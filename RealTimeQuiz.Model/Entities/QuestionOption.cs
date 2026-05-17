using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealTimeQuiz.Model.Entities;

[Table("QuestionOptions")]
public class QuestionOption
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int QuizQuestionId { get; set; }

    public QuizQuestion Question { get; set; } = null!;

    [Required]
    [MaxLength(300)]
    public required string Text { get; set; }

    public bool IsCorrect { get; set; }

    public int OrderIndex { get; set; }
}