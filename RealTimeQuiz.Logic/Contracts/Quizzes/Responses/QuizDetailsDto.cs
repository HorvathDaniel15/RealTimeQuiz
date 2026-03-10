namespace RealTimeQuiz.Logic.Contracts.Quizzes.Responses;

public class QuizDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public List<QuizQuestionDto> Questions { get; set; } = new();
}