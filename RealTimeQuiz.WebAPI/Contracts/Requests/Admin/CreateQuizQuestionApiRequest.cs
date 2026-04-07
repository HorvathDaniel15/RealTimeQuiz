using System.ComponentModel.DataAnnotations;

namespace RealTimeQuiz.WebAPI.Contracts.Requests.Admin;

public class CreateQuizQuestionApiRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Text { get; set; } = string.Empty;

    [Range(5, 300)]
    public int TimeLimitSeconds { get; set; } = 20;

    [StringLength(1000)]
    public string? ImageUrl { get; set; }

    [Required]
    [MinLength(2)]
    public List<CreateQuestionOptionApiRequest> Options { get; set; } = new();
}