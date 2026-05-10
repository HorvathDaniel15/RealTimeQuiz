using RealTimeQuiz.Logic.Contracts.Quizzes.Requests;
using RealTimeQuiz.Logic.Contracts.Quizzes.Responses;

namespace RealTimeQuiz.Logic.Services.Interfaces;

public interface IQuizService
{
    Task<IReadOnlyList<QuizListItemDto>> GetOwnQuizzesAsync(string ownerId, CancellationToken cancellationToken = default);
    Task<QuizDetailsDto> GetQuizDetailsAsync(int quizId, string ownerId, CancellationToken cancellationToken = default);
    Task<CreateQuizResultDto> CreateQuizAsync(CreateQuizRequest request, string ownerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaderboardEntryDto>> GetLeaderboardAsync(int quizId, CancellationToken cancellationToken = default);
}