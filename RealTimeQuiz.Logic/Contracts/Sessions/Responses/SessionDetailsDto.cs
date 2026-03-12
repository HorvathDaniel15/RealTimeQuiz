using RealTimeQuiz.Model.Entities;
using RealTimeQuiz.Model.Enums;

namespace RealTimeQuiz.Logic.Contracts.Sessions.Responses;

public class SessionDetailsDto
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public string JoinPin { get; set; } = string.Empty;
    public SessionState State { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? QuestionOpenedAtUtc { get; set; }
    public DateTime? QuestionClosedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    public int? CurrentQuestionId { get; set; }
    public int ParticipantCount { get; set; }
    public SessionCurrentQuestionDto? CurrentQuestion { get; set; }
}