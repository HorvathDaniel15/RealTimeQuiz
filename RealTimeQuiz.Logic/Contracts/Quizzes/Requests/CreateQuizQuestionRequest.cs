using System.ComponentModel.DataAnnotations;

namespace RealTimeQuiz.Logic.Contracts.Quizzes.Requests;

public class CreateQuizQuestionRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(1000)]
    public string Text { get; set; } = string.Empty;

    [Range(5, 300)]
    public int TimeLimitSeconds { get; set; } = 20;

    [StringLength(1000)]
    public string? ImageUrl { get; set; }

    [Required]
    [MinLength(2)]
    public List<CreateQuestionOptionRequest> Options { get; set; } = new();
}