using Microsoft.AspNetCore.Mvc;
using ProductWeb.Models;
using ProductWeb.Repositories;
using ProductWeb.Services;
using System;
using System.Threading.Tasks;

[Route("account")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserRepository _users;
    private readonly EmailService _emailService;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public AccountController(UserRepository userRepo, EmailService emailService, JwtTokenGenerator jwtTokenGenerator)
    {
        _users = userRepo;
        _emailService = emailService;
        _jwtTokenGenerator = jwtTokenGenerator;
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

        var verificationCode = GenerateVerificationCode();
        user.VerificationCode = verificationCode;
        _users.Update(user);

        await _emailService.SendEmailAsync(
            email,
            "Your login verification code",
            $"Your login verification code is: {verificationCode}"
        );

        return Ok(new { message = "Verification code sent to email." });
    }


    [HttpPost("verify")]
    public IActionResult Verify([FromForm] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Verification code is required.");

        var user = _users.GetByVerificationCode(code);
        if (user == null)
            return BadRequest("Invalid verification code.");

        user.IsVerified = true;
        user.VerificationCode = null;
        _users.Update(user);

        var token = _jwtTokenGenerator.GenerateToken(user);
        var bearerToken = "Bearer " + token;

        return Ok(new
        {
            message = "Verification successful. You are logged in.",
            token = bearerToken,
            user = new { user.Id, user.Username, user.Email, user.PhoneNumber, user.Role }
        });
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
