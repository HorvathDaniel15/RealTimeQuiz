using Microsoft.EntityFrameworkCore;
using RealTimeQuiz.Data.Interfaces;
using RealTimeQuiz.Model.Entities;

namespace RealTimeQuiz.Data.Repositories;

public class SessionAnswerRepository : ISessionAnswerRepository
{
    private readonly AppDbContext _context;

    public SessionAnswerRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<SessionAnswer?> GetByParticipantAndQuestionAsync(int participantId, int questionId, CancellationToken cancellationToken = default)
    {
        return await _context.SessionAnswers
            .FirstOrDefaultAsync(x => x.SessionParticipantId == participantId &&
                                      x.QuizQuestionId == questionId, cancellationToken);
    }

    public async Task<List<SessionAnswer>> GetAnswersForQuestionAsync(int sessionId, int questionId, CancellationToken cancellationToken = default)
    {
        return await _context.SessionAnswers
            .Include(x => x.SessionParticipant)
            .Where(x => x.QuizSessionId == sessionId && x.QuizQuestionId == questionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SessionAnswer>> GetAnswersForParticipantAsync(int participantId, CancellationToken cancellationToken = default)
    {
        return await _context.SessionAnswers
            .Include(x => x.QuizQuestion)
            .Include(x => x.QuestionOption)
            .Where(x => x.SessionParticipantId == participantId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SessionAnswer answer, CancellationToken cancellationToken = default)
    {
        await _context.SessionAnswers.AddAsync(answer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}