namespace RealTimeQuiz.Logic.Contracts.Quizzes.Responses;

public class QuestionOptionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsCorrect { get; set; }
}