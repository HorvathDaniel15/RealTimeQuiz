using Microsoft.AspNetCore.SignalR;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;
using RealTimeQuiz.WebAPI.SignalR.HubInterfaces;
using RealTimeQuiz.WebAPI.SignalR.Hubs;

namespace RealTimeQuiz.WebAPI.SignalR.Services;

public class SessionNotificationService : ISessionNotificationService
{
    private readonly IHubContext<SessionHub, ISessionClient> _hubContext;
    
    public SessionNotificationService(IHubContext<SessionHub, ISessionClient> hubContext)
    {
        _hubContext = hubContext;
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
}