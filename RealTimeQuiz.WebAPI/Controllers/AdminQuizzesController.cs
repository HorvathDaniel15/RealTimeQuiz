using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimeQuiz.Logic.Contracts.Quizzes.Requests;
using RealTimeQuiz.Logic.Contracts.Quizzes.Responses;
using RealTimeQuiz.Logic.Services.Interfaces;

namespace RealTimeQuiz.WebAPI.Controllers;

[ApiController]
[Route("api/admin/quizzes")]
public class AdminQuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;

    public AdminQuizzesController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<QuizListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<QuizListItemDto>>> GetOwn(
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Unauthorized();
        }
        
        var result = await _quizService.GetOwnQuizzesAsync(ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{quizId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(QuizDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuizDetailsDto>> GetById(
        int quizId,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Unauthorized();
        }
        
        var result = await _quizService.GetQuizDetailsAsync(quizId, ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateQuizResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateQuizResultDto>> Create(
        [FromBody] CreateQuizRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Unauthorized();
        }
        
        var result = await _quizService.CreateQuizAsync(request, ownerId, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { quizId = result.Id }, result);
    }
}
