using Microsoft.AspNetCore.Mvc;
using RealTimeQuiz.Logic.Contracts.Sessions.Requests;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.WebAPI.Contracts.Requests.Participant;
using RealTimeQuiz.WebAPI.Mappers;
using RealTimeQuiz.WebAPI.SignalR.Services;

namespace RealTimeQuiz.WebAPI.Controllers;

[ApiController]
[Route("api/participant-sessions")]
public class ParticipantSessionsController : ControllerBase
{
    private const string UserHeaderName = "X-User-Id";

    private readonly IParticipantSessionService _participantSessionService;
    private readonly ISessionNotificationService _notificationService;

    public ParticipantSessionsController(IParticipantSessionService participantSessionService, ISessionNotificationService notificationService)
    {
        _participantSessionService = participantSessionService;
        _notificationService = notificationService;
    }

    [HttpPost("join")]
    [ProducesResponseType(typeof(JoinSessionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JoinSessionResultDto>> Join(
        [FromBody] JoinSessionByPinApiRequest request,
        [FromHeader(Name = UserHeaderName)] string? userId,
        CancellationToken cancellationToken)
    {
        var joinRequest = request.ToLogicRequest();
        var result = await _participantSessionService.JoinByPinAsync(joinRequest, userId, cancellationToken);
        
        await _notificationService.NotifyParticipantJoinedAsync(result.SessionId, result);
        
        return Ok(result);
    }

    [HttpGet("{participantId:int}/current-question")]
    [ProducesResponseType(typeof(ParticipantCurrentQuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParticipantCurrentQuestionDto>> GetCurrentQuestion(
        int participantId,
        CancellationToken cancellationToken)
    {
        var request = new GetCurrentQuestionRequest
        {
            ParticipantId = participantId
        };

        var result = await _participantSessionService.GetCurrentQuestionForParticipantAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("submit-answer")]
    [ProducesResponseType(typeof(SubmitAnswerResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubmitAnswerResultDto>> SubmitAnswer(
        [FromBody] SubmitAnswerApiRequest request,
        CancellationToken cancellationToken)
    {
        var submitRequest = request.ToLogicRequest();
        var result = await _participantSessionService.SubmitAnswerAsync(submitRequest, cancellationToken);
        
        await _notificationService.NotifyAnswerSubmittedAsync(result.SessionId, result);
        
        return Ok(result);
    }

    [HttpGet("{participantId:int}/results/{questionId:int}")]
    [ProducesResponseType(typeof(SubmitAnswerResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubmitAnswerResultDto>> GetResult(
        int participantId,
        int questionId,
        CancellationToken cancellationToken)
    {
        var request = new GetParticipantAnswerResultRequest
        {
            ParticipantId = participantId,
            QuestionId = questionId
        };

        var result = await _participantSessionService.GetAnswerResultAsync(request, cancellationToken);
        return Ok(result);
    }
}

