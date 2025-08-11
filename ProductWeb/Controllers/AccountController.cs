using Microsoft.AspNetCore.Mvc;
using ProductWeb.Models;
using ProductWeb.Repositories;
using ProductWeb.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

[Route("account")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserRepository _users;
    private readonly EmailService _emailService;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtRepository _jwtRepository;

    public AccountController(
        UserRepository userRepo,
        EmailService emailService,
        JwtTokenGenerator jwtTokenGenerator,
        JwtRepository jwtRepository)
    {
        _users = userRepo;
        _emailService = emailService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _jwtRepository = jwtRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromForm] string username,
        [FromForm] string email,
        [FromForm] string phoneNumber,
        [FromForm] string password)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phoneNumber) ||
            string.IsNullOrWhiteSpace(password))
        {
            return BadRequest("All fields are required.");
        }

        if (_users.GetByEmail(email) != null)
            return BadRequest("Email already registered.");

        var verificationCode = GenerateVerificationCode();

        var user = new User
        {
            Username = username,
            Email = email,
            PhoneNumber = phoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            VerificationCode = verificationCode,
            IsVerified = false,
        };

        _users.Create(user);

        await _emailService.SendEmailAsync(
            email,
            "Your verification code",
            $"Your verification code is: {verificationCode}"
        );

        return Ok(new { message = "User created. Verification code sent to email." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromForm] string email,
        [FromForm] string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Email and password are required.");

        var user = _users.GetByEmail(email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        if (!user.IsVerified)
            return Unauthorized("User not verified.");

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var refreshToken = _jwtRepository.GenerateRefreshToken(ipAddress);

        await _jwtRepository.AddRefreshTokenAsync(user.Id, refreshToken);

        var accessToken = _jwtTokenGenerator.GenerateToken(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = refreshToken.ExpiresAt,
            SameSite = SameSiteMode.Strict
        };
        Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

        return Ok(new
        {
            message = "Login successful.",
            token = "Bearer " + accessToken,
            user = new { user.Id, user.Username, user.Email, user.PhoneNumber, user.Role }
        });
    }
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromForm] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Verification code is required.");

        var user = _users.GetByVerificationCode(code);
        if (user == null)
            return BadRequest("Invalid verification code.");

        user.IsVerified = true;
        user.VerificationCode = null;
        _users.Update(user);

        await _emailService.SendEmailAsync(
            user.Email,
            "Welcome to Ultron!",
            $"Hi {user.Username},\n\nThank you for verifying your account and joining Ultron. We're excited to have you with us!\n\nBest regards,\nThe Ultron Team"
        );

        var token = _jwtTokenGenerator.GenerateToken(user);
        var bearerToken = "Bearer " + token;

        return Ok(new
        {
            message = "Verification successful. You are logged in.",
            token = bearerToken,
            user = new { user.Id, user.Username, user.Email, user.PhoneNumber, user.Role }
        });
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized("Refresh token is missing.");

        var user = await _jwtRepository.GetByRefreshTokenAsync(refreshToken);
        if (user == null)
            return Unauthorized("Invalid refresh token.");

        var tokenEntry = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken && !rt.Revoked);
        if (tokenEntry == null || tokenEntry.ExpiresAt <= DateTime.UtcNow)
            return Unauthorized("Refresh token expired or revoked.");

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var newRefreshToken = _jwtRepository.GenerateRefreshToken(ipAddress);

        await _jwtRepository.ReplaceRefreshTokenAsync(user.Id, refreshToken, newRefreshToken);

        var accessToken = _jwtTokenGenerator.GenerateToken(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = newRefreshToken.ExpiresAt,
            SameSite = SameSiteMode.Strict
        };
        Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

        return Ok(new
        {
            token = "Bearer " + accessToken,
            refreshToken = newRefreshToken.Token
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest("Refresh token required.");

        var user = await _jwtRepository.GetByRefreshTokenAsync(refreshToken);
        if (user == null)
            return NotFound();

        await _jwtRepository.RevokeRefreshTokenAsync(user.Id, refreshToken);
        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logged out successfully." });
    }
    [HttpPost("password-reset-request")]
    public async Task<IActionResult> PasswordResetRequest([FromForm] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required.");

        var user = _users.GetByEmail(email);
        if (user == null)
            return BadRequest("Email not found.");

        var resetCode = GenerateVerificationCode();
        user.PasswordResetCode = resetCode;  
        _users.Update(user);

        await _emailService.SendEmailAsync(
            email,
            "Your Password Reset Code",
            $"Your password reset code is: {resetCode}"
        );

        return Ok(new { message = "Password reset code sent to your email." });
    }

    [HttpPost("password-reset")]
    public IActionResult PasswordReset([FromForm] string code, [FromForm] string newPassword)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPassword))
            return BadRequest("Code and new password are required.");

        var user = _users.GetByPasswordResetCode(code);  
        if (user == null)
            return BadRequest("Invalid password reset code.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetCode = null;
        _users.Update(user);

        return Ok(new { message = "Password has been reset successfully." });
    }
    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
