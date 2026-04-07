using System.ComponentModel.DataAnnotations;

namespace RealTimeQuiz.WebAPI.Contracts.Requests.Participant;

public class SubmitAnswerApiRequest
{
    [Range(1, int.MaxValue)]
    public int ParticipantId { get; set; }

    [Range(1, int.MaxValue)]
    public int QuestionId { get; set; }

    [Range(1, int.MaxValue)]
    public int OptionId { get; set; }
}

