using RealTimeQuiz.Model.Entities;

namespace RealTimeQuiz.Data.Interfaces;

public interface IParticipantRepository
{
    Task<SessionParticipant?> GetByIdAsync(int participantId, CancellationToken cancellationToken = default);
    Task<SessionParticipant?> GetBySessionAndNameAsync(int sessionId, string displayName, CancellationToken cancellationToken = default);
    Task<List<SessionParticipant>> GetBySessionAsync(int sessionId, CancellationToken cancellationToken = default);
    Task AddAsync(SessionParticipant participant, CancellationToken cancellationToken = default);
    Task UpdateAsync(SessionParticipant participant, CancellationToken cancellationToken = default);
}