using RealTimeQuiz.Logic.Contracts.Sessions.Requests;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;
using RealTimeQuiz.Logic.Services.Interfaces;

namespace RealTimeQuiz.Logic.Services.Implementations;

public class ParticipantSessionService : IParticipantSessionService
{
    public Task<JoinSessionResultDto> JoinByPinAsync(JoinSessionByPinRequest request, string? userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ParticipantCurrentQuestionDto> GetCurrentQuestionForParticipantAsync(GetCurrentQuestionRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SubmitAnswerResultDto> SubmitAnswerAsync(SubmitAnswerRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}