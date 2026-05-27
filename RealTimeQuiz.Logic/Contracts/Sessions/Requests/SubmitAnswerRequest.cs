using System.ComponentModel.DataAnnotations;

namespace RealTimeQuiz.Logic.Contracts.Sessions.Requests;

public class SubmitAnswerRequest
{
    [Range(1, int.MaxValue)]
    public int ParticipantId { get; set; }

    [Range(1, int.MaxValue)]
    public int QuestionId { get; set; }

    [Range(1, int.MaxValue)]
    public int OptionId { get; set; }
}