namespace RealTimeQuiz.Logic.Contracts.Quizzes.Responses;

public class CreateQuizResultDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime CreatedUtc { get; set; }
}