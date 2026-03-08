namespace RealTimeQuiz.Model.Entities;

public class QuizQuestion
{
    public int Id { get; set; }
    
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    
    public string Text { get; set; } = String.Empty;
    public int OrderIndex { get; set; }

    public int TimeLimitSeconds { get; set; } = 20;
    
    public string? ImageUrl { get; set; }
    
    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    public ICollection<SessionAnswer> Answers { get; set; } = new List<SessionAnswer>();
}