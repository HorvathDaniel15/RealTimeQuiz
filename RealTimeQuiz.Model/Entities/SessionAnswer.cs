using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RealTimeQuiz.Model.Enums;

namespace RealTimeQuiz.Model.Entities;

[Table("SessionAnswers")]
public class SessionAnswer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public int QuizSessionId { get; set; }
    public QuizSession QuizSession { get; set; } = null!;
    
    [Required]
    public int SessionParticipantId { get; set; }
    public SessionParticipant SessionParticipant { get; set; } = null!;
    
    [Required]
    public int QuizQuestionId { get; set; }
    public QuizQuestion QuizQuestion { get; set; } = null!;

    [Required]
    public int QuestionOptionId { get; set; }
    public QuestionOption QuestionOption { get; set; } = null!;

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Accepted;

    public bool IsCorrect { get; set; }
    
    
    public int AwardedPoints { get; set; }
}