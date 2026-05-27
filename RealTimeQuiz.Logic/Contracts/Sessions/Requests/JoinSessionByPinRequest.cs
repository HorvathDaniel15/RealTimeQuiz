using System.ComponentModel.DataAnnotations;

namespace RealTimeQuiz.Logic.Contracts.Sessions.Requests;

public class JoinSessionByPinRequest
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(6, MinimumLength = 6)]
    public string JoinPin { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;
}