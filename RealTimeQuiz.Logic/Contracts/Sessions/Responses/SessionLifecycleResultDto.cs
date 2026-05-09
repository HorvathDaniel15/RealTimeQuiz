using RealTimeQuiz.Model.Enums;

namespace RealTimeQuiz.Logic.Contracts.Sessions.Responses;

public class SessionLifecycleResultDto
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string JoinPin { get; set; } = string.Empty;
    public SessionState State { get; set; }
    public int? CurrentQuestionId { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? QuestionOpenedAtUtc { get; set; }
    public DateTime? QuestionClosedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public int? CurrentQuestionOrderIndex { get; set; }
    public string? CurrentQuestionText { get; set; }
    public int? CurrentQuestionTimeLimitSeconds { get; set; }
}