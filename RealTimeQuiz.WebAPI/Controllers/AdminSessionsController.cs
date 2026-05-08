using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.Model.Enums;
using RealTimeQuiz.WebAPI.Contracts.Requests.Admin;
using RealTimeQuiz.WebAPI.Mappers;
using RealTimeQuiz.WebAPI.SignalR.Services;

namespace RealTimeQuiz.WebAPI.Controllers;

[ApiController]
[Route("api/admin/sessions")]
public class AdminSessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ISessionNotificationService _notificationService;

    public AdminSessionsController(ISessionService sessionService, ISessionNotificationService notificationService)
    {
        _sessionService = sessionService;
        _notificationService = notificationService;
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
        
        await _notificationService.NotifyQuestionStartedAsync(sessionId, result);
        
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
        
        await _notificationService.NotifyQuestionClosedAsync(sessionId);
        
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

        if (result.State == SessionState.Finished)
        {
            await _notificationService.NotifySessionFinishedAsync(sessionId);
        }
        else
        {
            await _notificationService.NotifyQuestionStartedAsync(sessionId, result);
        }
        
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
        
        await _notificationService.NotifySessionFinishedAsync(sessionId);
        
        return Ok(result);
    }
}

