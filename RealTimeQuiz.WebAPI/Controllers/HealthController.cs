using Microsoft.AspNetCore.Mvc;

namespace RealTimeQuiz.WebAPI.Controllers;

public sealed record HealthRes(string Status);

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(HealthRes), StatusCodes.Status200OK)]
    public ActionResult<HealthRes> Get() => Ok(new { status = "ok" });
}