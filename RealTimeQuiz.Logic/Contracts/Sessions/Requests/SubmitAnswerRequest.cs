namespace RealTimeQuiz.Logic.Contracts.Sessions.Requests;

public class SubmitAnswerRequest
{
    public int ParticipantId { get; set; }
    public int QuestionId { get; set; }
    public int OptionId { get; set; }
}