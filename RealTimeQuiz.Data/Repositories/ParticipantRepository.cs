using Microsoft.EntityFrameworkCore;
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
        return await _context.SessionParticipants
            .FirstOrDefaultAsync(x => x.QuizSessionId == sessionId && 
                                      x.DisplayName == displayName, cancellationToken);
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

    public async Task UpdateAsync(SessionParticipant participant, CancellationToken cancellationToken = default)
    {
         _context.SessionParticipants.Update(participant);
         await _context.SaveChangesAsync(cancellationToken);
    }
}