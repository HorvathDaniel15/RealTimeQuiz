using Microsoft.AspNetCore.Identity;

namespace RealTimeQuiz.Model.Entities;

public class ApplicationUser : IdentityUser
{
    public ICollection<Quiz> OwnedQuizzes { get; set; } = new List<Quiz>();
    public ICollection<SessionParticipant> Participations { get; set; } = new List<SessionParticipant>();
}