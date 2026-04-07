namespace RealTimeQuiz.Logic.Contracts.Sessions.Requests;

public class GetParticipantAnswerResultRequest
{
    public int ParticipantId { get; set; }
    public int QuestionId { get; set; }
}

