using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealTimeQuiz.Model.Entities;

public class QuestionOption
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int QuizQuestionId { get; set; }
    public QuizQuestion QuizQuestion { get; set; } = null!;
    
    public string Text { get; set; } = String.Empty;
    public int OrderIndex { get; set; }
    
    public bool IsCorrect { get; set; }
}