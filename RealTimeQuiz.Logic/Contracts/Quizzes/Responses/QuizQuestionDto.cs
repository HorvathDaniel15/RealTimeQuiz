namespace RealTimeQuiz.Logic.Contracts.Quizzes.Responses;

public class QuizQuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int TimeLimitSeconds { get; set; }
    public string? ImageUrl { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
}