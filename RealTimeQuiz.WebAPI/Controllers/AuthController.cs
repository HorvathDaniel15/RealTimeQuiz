using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RealTimeQuiz.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public IActionResult Me()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");
        
        var userName =
            User.FindFirstValue(ClaimTypes.Name) ?? 
            User.Identity?.Name;
        
        var email = User.FindFirstValue(ClaimTypes.Email);

        return Ok(new
        {
           userId,
           userName,
           email,
           claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}