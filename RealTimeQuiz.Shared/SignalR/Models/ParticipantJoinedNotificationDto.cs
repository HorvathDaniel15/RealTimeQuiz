namespace RealTimeQuiz.Shared.SignalR.Models;

public class ParticipantJoinedNotificationDto
{
    public int SessionId { get; set; }
    public int ParticipantId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTime JoinedAtUtc { get; set; }
}
