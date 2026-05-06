using RealTimeQuiz.Model.Enums;

namespace RealTimeQuiz.Shared.SignalR.Models;

public class AnswerResultNotificationDto
{
    public int SessionId { get; set; }
    public int ParticipantId { get; set; }
    public int QuestionId { get; set; }
    public int OptionId { get; set; }
    public SubmissionStatus Status { get; set; }
    public bool? IsCorrect { get; set; }
    public int AwardedPoints { get; set; }
    public DateTime SubmittedAtUtc { get; set; }
}
