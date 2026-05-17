using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealTimeQuiz.Model.Entities;

[Table("SessionParticipants")]
public class SessionParticipant
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public int QuizSessionId { get; set; }
    public QuizSession QuizSession { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public required string DisplayName { get; set; }
    
    [MaxLength(450)]
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
    
    public int TotalScore { get; set; }
    
    public ICollection<SessionAnswer> Answers { get; set; } = new List<SessionAnswer>();
}