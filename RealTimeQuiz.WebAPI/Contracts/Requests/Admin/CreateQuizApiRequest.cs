using System.ComponentModel.DataAnnotations;

namespace RealTimeQuiz.WebAPI.Contracts.Requests.Admin;

public class CreateQuizApiRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateQuizQuestionApiRequest> Questions { get; set; } = new();
}

