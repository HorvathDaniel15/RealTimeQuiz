using RealTimeQuiz.Model.Enums;

namespace RealTimeQuiz.Logic.Contracts.Sessions.Responses;

public class CreateSessionResultDto
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string JoinPin { get; set; } = string.Empty;
    public SessionState State { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}