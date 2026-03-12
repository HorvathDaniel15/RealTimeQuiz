namespace RealTimeQuiz.Logic.Contracts.Sessions.Responses;

public class SessionCurrentQuestionDto
{
    public int Id { get; set; }
    public int OrderIndex { get; set; }
    public string Text { get; set; } = string.Empty;
    public int TimeLimitSeconds { get; set; }
    public string? ImageUrl { get; set; }
}