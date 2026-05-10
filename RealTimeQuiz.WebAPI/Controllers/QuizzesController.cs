using Microsoft.AspNetCore.Mvc;
using RealTimeQuiz.Logic.Contracts.Quizzes.Responses;
using RealTimeQuiz.Logic.Services.Interfaces;

namespace RealTimeQuiz.WebAPI.Controllers;

[ApiController]
[Route("api/quizzes")]
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizzesController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpGet("{quizId:int}/leaderboard")]
    [ProducesResponseType(typeof(IReadOnlyList<LeaderboardEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<LeaderboardEntryDto>>> GetLeaderboard(
        int quizId,
        CancellationToken cancellationToken)
    {
        var result = await _quizService.GetLeaderboardAsync(quizId, cancellationToken);
        return Ok(result);
    }
}

