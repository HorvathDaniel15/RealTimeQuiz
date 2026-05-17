using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealTimeQuiz.Model.Entities;

[Table("QuizQuestions")]
public class QuizQuestion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int QuizId { get; set; }

    public Quiz Quiz { get; set; } = null!;

    [Required]
    [MaxLength(1000)]
    public required string Text { get; set; }

    [MaxLength(1000)]
    public string? ImageUrl { get; set; }

    public int OrderIndex { get; set; }

    public int TimeLimitSeconds { get; set; } = 20;

    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    public ICollection<SessionAnswer> Answers { get; set; } = new List<SessionAnswer>();
}