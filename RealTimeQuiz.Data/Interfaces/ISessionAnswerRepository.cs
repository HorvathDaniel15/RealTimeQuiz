using RealTimeQuiz.Model.Entities;

namespace RealTimeQuiz.Data.Interfaces;

public interface ISessionAnswerRepository
{
    Task<SessionAnswer?> GetByParticipantAndQuestionAsync(int participantId, int questionId, CancellationToken cancellationToken = default);
    Task<List<SessionAnswer>> GetAnswersForQuestionAsync(int sessionId, int questionId, CancellationToken cancellationToken = default);
    Task<List<SessionAnswer>> GetAnswersForParticipantAsync(int participantId, CancellationToken cancellationToken = default);
    Task AddAsync(SessionAnswer answer, CancellationToken cancellationToken = default);
    Task<bool> TryAddAsync(SessionAnswer answer, CancellationToken cancellationToken = default);
}