using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealTimeQuiz.Model.Entities;

public class Quiz
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string Title { get; set; } = String.Empty;
    public string? Description { get; set; }
    
    public string OwnerId { get; set; } = String.Empty;
    public ApplicationUser Owner { get; set; } = null!;
    
    public bool IsPublished { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    
    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    public ICollection<QuizSession> Sessions { get; set; } = new List<QuizSession>();
}