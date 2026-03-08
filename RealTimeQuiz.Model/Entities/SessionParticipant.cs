namespace RealTimeQuiz.Model.Entities;

public class SessionParticipant
{
    public int Id { get; set; }
    
    public int QuizSessionId { get; set; }
    public QuizSession QuizSession { get; set; } = null!;
    
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public string DisplayName { get; set; } = String.Empty;
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
    
    public int TotalScore { get; set; }
    
    public ICollection<SessionAnswer> Answers { get; set; } = new List<SessionAnswer>();
}