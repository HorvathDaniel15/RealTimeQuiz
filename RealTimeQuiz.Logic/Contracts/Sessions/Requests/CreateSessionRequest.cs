using System.ComponentModel.DataAnnotations;

namespace RealTimeQuiz.Logic.Contracts.Sessions.Requests;

public class CreateSessionRequest
{
    [Range(1, int.MaxValue)]
    public int QuizId { get; set; }
}