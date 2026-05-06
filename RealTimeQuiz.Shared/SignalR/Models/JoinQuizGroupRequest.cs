using System.ComponentModel.DataAnnotations;

namespace RealTimeQuiz.Shared.SignalR.Models;

public class JoinQuizGroupRequest
{
    [Range(1, int.MaxValue)]
    public int SessionId { get; set; }
    
    [Range(1, int.MaxValue)]
    public int? ParticipantId { get; set; }
}
