using RealTimeQuiz.Logic.Contracts.Quizzes.Responses;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;

namespace RealTimeQuiz.WebAPI.SignalR.HubInterfaces;

public interface ISessionClient
{
    Task QuestionStarted(SessionLifecycleResultDto sessionData);
    Task QuestionClosed();
    Task SessionFinished();
    Task ParticipantJoined(JoinSessionResultDto participantData);
    Task AnswerSubmitted(SubmitAnswerResultDto answerData);
    Task LeaderboardUpdated(IReadOnlyList<LeaderboardEntryDto> leaderboard);
}