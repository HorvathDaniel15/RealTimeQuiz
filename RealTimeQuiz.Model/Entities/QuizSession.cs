using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RealTimeQuiz.Model.Enums;

namespace RealTimeQuiz.Model.Entities;

[Table("QuizSessions")]
public class QuizSession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
    
    [Required]
    [MaxLength(20)]
    public string JoinPin { get; set; } = String.Empty;

    public SessionState State { get; set; } = SessionState.Draft;
    
    public int? CurrentQuestionId { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? QuestionOpenedAtUtc { get; set; }
    public DateTime? QuestionClosedAtUtc { get; set; }
    public DateTime? FinishedAtUtc { get; set; }
    
    public ICollection<SessionParticipant> Participants { get; set; } = new List<SessionParticipant>();
    public ICollection<SessionAnswer> Answers { get; set; } = new List<SessionAnswer>();
}