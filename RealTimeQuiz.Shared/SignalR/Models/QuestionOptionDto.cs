namespace RealTimeQuiz.Shared.SignalR.Models;

public class QuestionOptionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}
