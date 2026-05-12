using Microsoft.AspNetCore.Mvc;
using RealTimeQuiz.Logic.Contracts.Quizzes.Responses;
using RealTimeQuiz.Logic.Services.Interfaces;

namespace RealTimeQuiz.WebAPI.Controllers;

[ApiController]
[Route("api/sessions")]
public class QuizzesController : ControllerBase
{
    private readonly IParticipantSessionService _sessionService;

    public QuizzesController(IParticipantSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet("{sessionId:int}/leaderboard")]
    [ProducesResponseType(typeof(IReadOnlyList<LeaderboardEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<LeaderboardEntryDto>>> GetLeaderboard(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var result = await _sessionService.GetLeaderboardForSessionAsync(sessionId, cancellationToken);
        return Ok(result);
    }
}
