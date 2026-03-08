using Microsoft.EntityFrameworkCore;
using RealTimeQuiz.Data.Interfaces;
using RealTimeQuiz.Model.Entities;

namespace RealTimeQuiz.Data.Repositories;

public class QuizRepository : IQuizRepository
{
    private readonly AppDbContext _context;
    
    public QuizRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Quiz>> GetByOwnerAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .Where(q => q.OwnerId == ownerId)
            .OrderByDescending(q => q.CreatedUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Quiz?> GetByIdAsync(int quizId, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .FirstOrDefaultAsync(q=> q.Id == quizId, cancellationToken);
    }

    public async Task<Quiz?> GetDetailedByIdAsync(int quizId, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .Include(q => q.Questions.OrderBy(x=> x.OrderIndex))
                .ThenInclude(q => q.Options.OrderBy(x=> x.OrderIndex))
            .FirstOrDefaultAsync(q=> q.Id == quizId, cancellationToken);
    }

    public async Task AddAsync(Quiz quiz, CancellationToken cancellationToken = default)
    {
        await _context.Quizzes.AddAsync(quiz, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Quiz quiz, CancellationToken cancellationToken = default)
    {
        _context.Quizzes.Update(quiz);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Quiz quiz, CancellationToken cancellationToken = default)
    {
        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int quizId, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .AnyAsync(q => q.Id == quizId, cancellationToken);
    }
}