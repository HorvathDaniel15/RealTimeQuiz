using RealTimeQuiz.Model.Enums;

namespace RealTimeQuiz.Logic.Contracts.Sessions.Responses;

public class SubmitAnswerResultDto
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