using RealTimeQuiz.Shared.SignalR.Models;

namespace RealTimeQuiz.Shared.SignalR.HubInterfaces;

public interface IQuizClient
{
    Task SessionStateChanged(SessionStateChangedNotificationDto notification);
    Task ParticipantJoined(ParticipantJoinedNotificationDto notification);
    Task CurrentQuestionOpened(CurrentQuestionNotificationDto notification);
    Task AnswerResultAvailable(AnswerResultNotificationDto notification);
}
