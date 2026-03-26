namespace RealTimeQuiz.Logic.Contracts.Sessions.Responses;

public class ParticipantCurrentQuestionDto
{
    public int SessionId { get; set; }
    public int QuestionId { get; set; }
    public int OrderIndex { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? TimeLimitSeconds { get; set; }
    public DateTime OpenedAtUtc { get; set; }
    public IReadOnlyList<ParticipantQuestionOptionDto> Options { get; set; } = [];
}