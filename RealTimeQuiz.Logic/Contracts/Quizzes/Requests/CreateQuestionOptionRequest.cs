

namespace RealTimeQuiz.Logic.Contracts.Quizzes.Requests;

public class CreateQuestionOptionRequest
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}