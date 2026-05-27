using System.ComponentModel.DataAnnotations;

namespace RealTimeQuiz.Logic.Contracts.Quizzes.Requests;

public class CreateQuestionOptionRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(300)]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}