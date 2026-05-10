namespace RealTimeQuiz.Data.Models;
public class ParticipantScoreModel
{
    public required string ParticipantName { get; init; }
    public required int CorrectAnswersCount { get; init; }
}
