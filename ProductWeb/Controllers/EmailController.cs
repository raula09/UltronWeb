using Microsoft.AspNetCore.Mvc;

[Route("test-email")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly EmailService _emailService;

    public EmailController(EmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpGet]
  
    public async Task<IActionResult> SendTest()
    {
        await _emailService.SendEmailAsync("your-real-email@example.com", "Hello!", "<h1>This is a test email</h1>");
        return Ok("Email sent.");
    }

}
