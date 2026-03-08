using Microsoft.EntityFrameworkCore;
using RealTimeQuiz.Data.Interfaces;
using RealTimeQuiz.Model.Entities;

namespace RealTimeQuiz.Data.Repositories;

public class QuizSessionRepository : IQuizSessionRepository
{
    private readonly AppDbContext _context;

    public QuizSessionRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<QuizSession?> GetByIdAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.QuizSessions
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
    }

    public async Task<QuizSession?> GetByPinAsync(string joinPin, CancellationToken cancellationToken = default)
    {
        return await _context.QuizSessions
            .FirstOrDefaultAsync(x=> x.JoinPin == joinPin, cancellationToken);
    }

    public async Task<QuizSession?> GetDetailedByIdAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.QuizSessions
            .Include(x=> x.Quiz)
                .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.Options)
            .Include(x => x.Participants)
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
    }

    public async Task AddAsync(QuizSession session, CancellationToken cancellationToken = default)
    {
        await  _context.QuizSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(QuizSession session, CancellationToken cancellationToken = default)
    {
        _context.QuizSessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> JoinPinExistsAsync(string joinPin, CancellationToken cancellationToken = default)
    {
        return await _context.QuizSessions
            .AnyAsync(x => x.JoinPin == joinPin, cancellationToken);
    }
}