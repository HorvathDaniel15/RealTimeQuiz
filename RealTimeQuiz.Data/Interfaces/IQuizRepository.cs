using RealTimeQuiz.Data.Models;
using RealTimeQuiz.Model.Entities;

namespace RealTimeQuiz.Data.Interfaces;

public interface IQuizRepository
{
    Task<List<Quiz>> GetByOwnerAsync(string ownerId, CancellationToken cancellationToken = default);
    Task<Quiz?> GetByIdAsync(int quizId, CancellationToken cancellationToken = default);
    Task<Quiz?> GetDetailedByIdAsync(int quizId, CancellationToken cancellationToken = default);
    Task AddAsync(Quiz quiz, CancellationToken cancellationToken = default);
    Task UpdateAsync(Quiz quiz, CancellationToken cancellationToken = default);
    Task DeleteAsync(Quiz quiz, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int quizId, CancellationToken cancellationToken = default);
}