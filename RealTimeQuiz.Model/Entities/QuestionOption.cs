namespace RealTimeQuiz.Model.Entities;

public class QuestionOption
{
    public int Id { get; set; }
    
    public int QuestionId { get; set; }
    public QuizQuestion QuizQuestion { get; set; } = null!;
    
    public string Text { get; set; } = String.Empty;
    public int OrderIndex { get; set; }
    
    public bool IsCorrect { get; set; }
}