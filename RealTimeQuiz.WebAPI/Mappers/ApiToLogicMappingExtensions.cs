using RealTimeQuiz.Logic.Contracts.Quizzes.Requests;
using RealTimeQuiz.Logic.Contracts.Sessions.Requests;
using RealTimeQuiz.WebAPI.Contracts.Requests.Admin;
using RealTimeQuiz.WebAPI.Contracts.Requests.Participant;

namespace RealTimeQuiz.WebAPI.Mappers;

public static class ApiToLogicMappingExtensions
{
    public static CreateQuizRequest ToLogicRequest(this CreateQuizApiRequest request)
    {
        return new CreateQuizRequest
        {
            Title = request.Title,
            Description = request.Description,
            Questions = request.Questions
                .Select(x => new CreateQuizQuestionRequest
                {
                    Text = x.Text,
                    TimeLimitSeconds = x.TimeLimitSeconds,
                    ImageUrl = x.ImageUrl,
                    Options = x.Options
                        .Select(o => new CreateQuestionOptionRequest
                        {
                            Text = o.Text,
                            IsCorrect = o.IsCorrect
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    public static CreateSessionRequest ToLogicRequest(this CreateSessionApiRequest request)
    {
        return new CreateSessionRequest
        {
            QuizId = request.QuizId
        };
    }

    public static JoinSessionByPinRequest ToLogicRequest(this JoinSessionByPinApiRequest request)
    {
        return new JoinSessionByPinRequest
        {
            JoinPin = request.JoinPin,
            DisplayName = request.DisplayName
        };
    }

    public static SubmitAnswerRequest ToLogicRequest(this SubmitAnswerApiRequest request)
    {
        return new SubmitAnswerRequest
        {
            ParticipantId = request.ParticipantId,
            QuestionId = request.QuestionId,
            OptionId = request.OptionId
        };
    }
}

