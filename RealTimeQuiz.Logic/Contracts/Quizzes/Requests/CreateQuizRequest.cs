namespace RealTimeQuiz.Logic.Contracts.Quizzes.Requests;

public class CreateQuizRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateQuizQuestionRequest> Questions { get; set; } = new();
}