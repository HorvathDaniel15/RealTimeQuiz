using Microsoft.AspNetCore.Mvc;
using RealTimeQuiz.Logic.Contracts.Quizzes.Responses;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.WebAPI.Contracts.Requests.Admin;
using RealTimeQuiz.WebAPI.Mappers;

namespace RealTimeQuiz.WebAPI.Controllers;

[ApiController]
[Route("api/admin/quizzes")]
public class AdminQuizzesController : ControllerBase
{
    private const string OwnerHeaderName = "X-Owner-Id";

    private readonly IQuizService _quizService;

    public AdminQuizzesController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<QuizListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<QuizListItemDto>>> GetOwn(
        [FromHeader(Name = OwnerHeaderName)] string ownerId,
        CancellationToken cancellationToken)
    {
        var result = await _quizService.GetOwnQuizzesAsync(ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{quizId:int}")]
    [ProducesResponseType(typeof(QuizDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QuizDetailsDto>> GetById(
        int quizId,
        [FromHeader(Name = OwnerHeaderName)] string ownerId,
        CancellationToken cancellationToken)
    {
        var result = await _quizService.GetQuizDetailsAsync(quizId, ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateQuizResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateQuizResultDto>> Create(
        [FromBody] CreateQuizApiRequest request,
        [FromHeader(Name = OwnerHeaderName)] string ownerId,
        CancellationToken cancellationToken)
    {
        var createRequest = request.ToLogicRequest();
        var result = await _quizService.CreateQuizAsync(createRequest, ownerId, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { quizId = result.Id }, result);
    }
}

