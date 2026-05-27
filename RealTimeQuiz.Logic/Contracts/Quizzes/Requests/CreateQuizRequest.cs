using System.ComponentModel.DataAnnotations;

namespace RealTimeQuiz.Logic.Contracts.Quizzes.Requests;

public class CreateQuizRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateQuizQuestionRequest> Questions { get; set; } = new();
}