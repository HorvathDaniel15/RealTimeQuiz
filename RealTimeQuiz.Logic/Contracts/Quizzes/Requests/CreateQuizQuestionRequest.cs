namespace RealTimeQuiz.Logic.Contracts.Quizzes.Requests;

public class CreateQuizQuestionRequest
{
    public string Text { get; set; } = string.Empty;
    public int TimeLimitSeconds { get; set; } = 20;
    public string? ImageUrl { get; set; }
    public List<CreateQuestionOptionRequest> Options { get; set; } = new();
}