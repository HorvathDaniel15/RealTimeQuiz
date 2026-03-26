using Microsoft.EntityFrameworkCore;
using Npgsql;
using RealTimeQuiz.Data.Interfaces;
using RealTimeQuiz.Model.Entities;

namespace RealTimeQuiz.Data.Repositories;

public class ParticipantRepository : IParticipantRepository
{
    private readonly AppDbContext _context;

    public ParticipantRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<SessionParticipant?> GetByIdAsync(int participantId, CancellationToken cancellationToken = default)
    {
        return await _context.SessionParticipants
            .FirstOrDefaultAsync(x => x.Id == participantId, cancellationToken);
    }

    public async Task<SessionParticipant?> GetBySessionAndNameAsync(int sessionId, string displayName, CancellationToken cancellationToken = default)
    {
        var normalizedDisplayName = displayName.Trim().ToLower();

        return await _context.SessionParticipants
            .FirstOrDefaultAsync(x => x.QuizSessionId == sessionId
                                      && x.DisplayName.Trim().ToLower() == normalizedDisplayName,
                cancellationToken);
    }

    public async Task<List<SessionParticipant>> GetBySessionAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.SessionParticipants
            .Where(x => x.QuizSessionId == sessionId)
            .OrderByDescending(x => x.TotalScore)
            .ThenBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SessionParticipant participant, CancellationToken cancellationToken = default)
    {
        await _context.SessionParticipants.AddAsync(participant, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> TryAddAsync(SessionParticipant participant, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SessionParticipants.AddAsync(participant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (IsSessionDisplayNameUniqueConstraintViolation(ex))
        {
            // Keep context clean after failed insert so the caller can continue safely.
            _context.Entry(participant).State = EntityState.Detached;
            return false;
        }
    }

    public async Task UpdateAsync(SessionParticipant participant, CancellationToken cancellationToken = default)
    {
         _context.SessionParticipants.Update(participant);
         await _context.SaveChangesAsync(cancellationToken);
    }

    private static bool IsSessionDisplayNameUniqueConstraintViolation(DbUpdateException exception)
    {
        if (exception.InnerException is not PostgresException postgresException)
        {
            return false;
        }

        return postgresException.SqlState == PostgresErrorCodes.UniqueViolation
               && string.Equals(postgresException.ConstraintName,
                   "IX_SessionParticipants_QuizSessionId_DisplayName",
                   StringComparison.Ordinal);
    }
}