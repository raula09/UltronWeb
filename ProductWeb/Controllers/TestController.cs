using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("test")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public() => Ok("This endpoint is public.");

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly() => Ok("Hello Admin! You have access.");
}
