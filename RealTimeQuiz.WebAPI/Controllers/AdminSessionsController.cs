using Microsoft.AspNetCore.Mvc;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.WebAPI.Contracts.Requests.Admin;
using RealTimeQuiz.WebAPI.Mappers;

namespace RealTimeQuiz.WebAPI.Controllers;

[ApiController]
[Route("api/admin/sessions")]
public class AdminSessionsController : ControllerBase
{
    private const string OwnerHeaderName = "X-Owner-Id";

    private readonly ISessionService _sessionService;

    public AdminSessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateSessionResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateSessionResultDto>> Create(
        [FromBody] CreateSessionApiRequest request,
        [FromHeader(Name = OwnerHeaderName)] string ownerId,
        CancellationToken cancellationToken)
    {
        var createRequest = request.ToLogicRequest();
        var result = await _sessionService.CreateSessionAsync(createRequest, ownerId, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { sessionId = result.Id }, result);
    }

    [HttpGet("{sessionId:int}")]
    [ProducesResponseType(typeof(SessionDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionDetailsDto>> GetById(
        int sessionId,
        [FromHeader(Name = OwnerHeaderName)] string ownerId,
        CancellationToken cancellationToken)
    {
        var result = await _sessionService.GetSessionDetailsAsync(sessionId, ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:int}/open-lobby")]
    [ProducesResponseType(typeof(SessionLifecycleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionLifecycleResultDto>> OpenLobby(
        int sessionId,
        [FromHeader(Name = OwnerHeaderName)] string ownerId,
        CancellationToken cancellationToken)
    {
        var result = await _sessionService.OpenLobbyAsync(sessionId, ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:int}/start")]
    [ProducesResponseType(typeof(SessionLifecycleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionLifecycleResultDto>> Start(
        int sessionId,
        [FromHeader(Name = OwnerHeaderName)] string ownerId,
        CancellationToken cancellationToken)
    {
        var result = await _sessionService.StartSessionAsync(sessionId, ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:int}/close-current-question")]
    [ProducesResponseType(typeof(SessionLifecycleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionLifecycleResultDto>> CloseCurrentQuestion(
        int sessionId,
        [FromHeader(Name = OwnerHeaderName)] string ownerId,
        CancellationToken cancellationToken)
    {
        var result = await _sessionService.CloseCurrentQuestionAsync(sessionId, ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:int}/advance")]
    [ProducesResponseType(typeof(SessionLifecycleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionLifecycleResultDto>> Advance(
        int sessionId,
        [FromHeader(Name = OwnerHeaderName)] string ownerId,
        CancellationToken cancellationToken)
    {
        var result = await _sessionService.AdvanceToNextQuestionAsync(sessionId, ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:int}/finish")]
    [ProducesResponseType(typeof(SessionLifecycleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionLifecycleResultDto>> Finish(
        int sessionId,
        [FromHeader(Name = OwnerHeaderName)] string ownerId,
        CancellationToken cancellationToken)
    {
        var result = await _sessionService.FinishSessionAsync(sessionId, ownerId, cancellationToken);
        return Ok(result);
    }
}

