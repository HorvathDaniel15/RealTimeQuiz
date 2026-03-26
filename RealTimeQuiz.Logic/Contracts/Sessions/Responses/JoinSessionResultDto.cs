using RealTimeQuiz.Model.Enums;

namespace RealTimeQuiz.Logic.Contracts.Sessions.Responses;

public class JoinSessionResultDto
{
    public int ParticipantId { get; set; }
    public int SessionId { get; set; }
    public string JoinPin { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public SessionState SessionState { get; set; }
    public DateTime JoinedAtUtc { get; set; }
}