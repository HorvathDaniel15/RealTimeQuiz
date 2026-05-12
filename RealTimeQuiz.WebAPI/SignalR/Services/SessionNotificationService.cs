using Microsoft.AspNetCore.SignalR;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.WebAPI.SignalR.HubInterfaces;
using RealTimeQuiz.WebAPI.SignalR.Hubs;

namespace RealTimeQuiz.WebAPI.SignalR.Services;

public class SessionNotificationService : ISessionNotificationService
{
    private readonly IHubContext<SessionHub, ISessionClient> _hubContext;
    private readonly IParticipantSessionService _participantSessionService;
    
    public SessionNotificationService(
        IHubContext<SessionHub, ISessionClient> hubContext, 
        IParticipantSessionService participantSessionService)
    {
        _hubContext = hubContext;
        _participantSessionService = participantSessionService;
    }
    
    public async Task AddToGroupAsync(string connectionId, string groupName)
    {
        await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
    }

    public async Task RemoveFromGroupAsync(string connectionId, string groupName)
    {
        await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
    }
    
    public async Task NotifyQuestionStartedAsync(int sessionId, SessionLifecycleResultDto sessionData)
    {
        await _hubContext.Clients.Group($"Session_{sessionId}").QuestionStarted(sessionData);
    }

    public async Task NotifyQuestionClosedAsync(int sessionId)
    {
        await _hubContext.Clients.Group($"Session_{sessionId}").QuestionClosed();
    }

    public async Task NotifySessionFinishedAsync(int sessionId)
    {
        await _hubContext.Clients.Group($"Session_{sessionId}").SessionFinished();
    }

    public async Task NotifyParticipantJoinedAsync(int sessionId, JoinSessionResultDto participantData)
    {
        await _hubContext.Clients.Group($"Session_{sessionId}").ParticipantJoined(participantData);
    }

    public async Task NotifyAnswerSubmittedAsync(int sessionId, SubmitAnswerResultDto answerData)
    {
        await _hubContext.Clients.Group($"Session_{sessionId}").AnswerSubmitted(answerData);
    }

    public async Task NotifyLeaderboardUpdatedAsync(int sessionId)
    {
        var result = await _participantSessionService.GetLeaderboardForSessionAsync(sessionId, CancellationToken.None);
        await _hubContext.Clients.Group($"Session_{sessionId}").LeaderboardUpdated(result);
    }

    public async Task SendInitialLeaderboardAsync(string connectionId, int sessionId)
    {
        var result = await _participantSessionService.GetLeaderboardForSessionAsync(sessionId, CancellationToken.None);
        await _hubContext.Clients.Client(connectionId).LeaderboardUpdated(result);
    }
}