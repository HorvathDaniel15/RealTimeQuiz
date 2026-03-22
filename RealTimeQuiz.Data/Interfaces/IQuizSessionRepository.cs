using RealTimeQuiz.Model.Entities;

namespace RealTimeQuiz.Data.Interfaces;

public interface IQuizSessionRepository
{
    Task<QuizSession?> GetByIdAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<QuizSession?> GetByPinAsync(string joinPin, CancellationToken cancellationToken = default);
    Task<QuizSession?> GetDetailedByIdAsync(int sessionId, CancellationToken cancellationToken = default);
    
    Task AddAsync(QuizSession session, CancellationToken cancellationToken = default);
    Task<bool> TryAddAsync(QuizSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(QuizSession session, CancellationToken cancellationToken = default);
    
    Task<bool> JoinPinExistsAsync(string joinPin, CancellationToken cancellationToken = default);
}