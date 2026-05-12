using RealTimeQuiz.Logic.Contracts.Sessions.Responses;

namespace RealTimeQuiz.WebAPI.SignalR.Services;

public interface ISessionNotificationService
{
    Task AddToGroupAsync(string connectionId, string groupName);
    Task RemoveFromGroupAsync(string connectionId, string groupName);
    
    Task NotifyQuestionStartedAsync(int sessionId, SessionLifecycleResultDto sessionData);
    Task NotifyQuestionClosedAsync(int sessionId);
    Task NotifySessionFinishedAsync(int sessionId);
    Task NotifyParticipantJoinedAsync(int sessionId, JoinSessionResultDto participantData);
    Task NotifyAnswerSubmittedAsync(int sessionId, SubmitAnswerResultDto answerData);

    Task NotifyLeaderboardUpdatedAsync(int sessionId);
    
    Task SendInitialLeaderboardAsync(string connectionId, int sessionId);
}