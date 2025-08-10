using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using ProductWeb.Data;
using ProductWeb.Models;
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
    [FromForm] string password)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return BadRequest("Username, email and password are required.");
        }

        if (_users.GetByEmail(email) != null)
            return BadRequest("Email already registered.");

        var verificationCode = new Random().Next(100000, 999999).ToString();

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),  
            VerificationCode = verificationCode,
            IsVerified = false
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
    public IActionResult Login([FromForm] LoginFormRequest request)
    {
        var user = _users.GetByEmail(request.Email);
        if (user == null || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(user.PasswordHash) ||
      !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials");
        }

        if (!user.IsVerified)
        {
            if (string.IsNullOrWhiteSpace(request.LoginVerificationCode))
                return Unauthorized("Email not verified. Please provide verification code.");

            if (user.VerificationCode != request.LoginVerificationCode)
                return Unauthorized("Invalid verification code.");

            user.IsVerified = true;
            user.VerificationCode = null;
            _users.Update(user);
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return Ok(new
        {
            token,
            user = new { user.Id, user.Username, user.Email, user.Role }
        });
    }

}
