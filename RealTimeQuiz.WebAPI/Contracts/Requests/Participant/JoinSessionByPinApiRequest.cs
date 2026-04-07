using System.ComponentModel.DataAnnotations;

namespace RealTimeQuiz.WebAPI.Contracts.Requests.Participant;

public class JoinSessionByPinApiRequest
{
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string JoinPin { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string DisplayName { get; set; } = string.Empty;
}

