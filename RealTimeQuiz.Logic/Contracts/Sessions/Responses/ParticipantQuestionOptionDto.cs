namespace RealTimeQuiz.Logic.Contracts.Sessions.Responses;

public class ParticipantQuestionOptionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}