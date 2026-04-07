using System.ComponentModel.DataAnnotations;

namespace RealTimeQuiz.WebAPI.Contracts.Requests.Admin;

public class CreateSessionApiRequest
{
    [Range(1, int.MaxValue)]
    public int QuizId { get; set; }
}

