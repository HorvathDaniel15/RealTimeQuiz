using RealTimeQuiz.Logic.Contracts.Sessions.Requests;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;
using RealTimeQuiz.Logic.Contracts.Quizzes.Responses;

namespace RealTimeQuiz.Logic.Services.Interfaces;

public interface IParticipantSessionService
{
    Task<JoinSessionResultDto> JoinByPinAsync(
        JoinSessionByPinRequest request,
        string? userId,
        CancellationToken cancellationToken =  default);

    Task<ParticipantCurrentQuestionDto> GetCurrentQuestionForParticipantAsync(
        GetCurrentQuestionRequest request,
        CancellationToken cancellationToken = default);
    
    Task<SubmitAnswerResultDto> SubmitAnswerAsync(
        SubmitAnswerRequest request,
        CancellationToken cancellationToken = default);

    Task<SubmitAnswerResultDto> GetAnswerResultAsync(
        GetParticipantAnswerResultRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaderboardEntryDto>> GetLeaderboardForSessionAsync(
        int sessionId, 
        CancellationToken cancellationToken = default);
}