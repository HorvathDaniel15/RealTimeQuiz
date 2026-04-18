using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
    private readonly ISessionService _sessionService;

    public AdminSessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateSessionResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateSessionResultDto>> Create(
        [FromBody] CreateSessionApiRequest request,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Unauthorized();
        }
        var createRequest = request.ToLogicRequest();
        var result = await _sessionService.CreateSessionAsync(createRequest, ownerId, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { sessionId = result.Id }, result);
    }

    [HttpGet("{sessionId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(SessionDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionDetailsDto>> GetById(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Unauthorized();
        }
        var result = await _sessionService.GetSessionDetailsAsync(sessionId, ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:int}/open-lobby")]
    [Authorize]
    [ProducesResponseType(typeof(SessionLifecycleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionLifecycleResultDto>> OpenLobby(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      User.FindFirstValue("sub");
        
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Unauthorized();
        }
        var result = await _sessionService.OpenLobbyAsync(sessionId, ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:int}/start")]
    [Authorize]
    [ProducesResponseType(typeof(SessionLifecycleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionLifecycleResultDto>> Start(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var  ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                       User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Unauthorized();
        }
        var result = await _sessionService.StartSessionAsync(sessionId, ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:int}/close-current-question")]
    [Authorize]
    [ProducesResponseType(typeof(SessionLifecycleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionLifecycleResultDto>> CloseCurrentQuestion(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      User.FindFirstValue("sub");
        
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Unauthorized();
        }
        var result = await _sessionService.CloseCurrentQuestionAsync(sessionId, ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:int}/advance")]
    [Authorize]
    [ProducesResponseType(typeof(SessionLifecycleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionLifecycleResultDto>> Advance(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      User.FindFirstValue("sub");
        
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Unauthorized();
        }
        var result = await _sessionService.AdvanceToNextQuestionAsync(sessionId, ownerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{sessionId:int}/finish")]
    [Authorize]
    [ProducesResponseType(typeof(SessionLifecycleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionLifecycleResultDto>> Finish(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                      User.FindFirstValue("sub");
        
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return Unauthorized();
        }
        var result = await _sessionService.FinishSessionAsync(sessionId, ownerId, cancellationToken);
        return Ok(result);
    }
}

