namespace RealTimeQuiz.Logic.Contracts.Quizzes.Responses;
public class LeaderboardEntryDto
{
    public int Position { get; set; }
    public required string ParticipantName { get; init; }
    public int CorrectAnswersCount { get; init; }
}
