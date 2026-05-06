using RealTimeQuiz.Model.Enums;

namespace RealTimeQuiz.Shared.SignalR.Models;

public class SessionStateChangedNotificationDto
{
    public int SessionId { get; set; }
    public SessionState State { get; set; }
    public int? CurrentQuestionId { get; set; }
    public int? CurrentQuestionOrderIndex { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? QuestionOpenedAtUtc { get; set; }
    public DateTime? QuestionClosedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
}
