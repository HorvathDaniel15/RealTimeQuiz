using RealTimeQuiz.Logic.Contracts.Sessions.Requests;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;

namespace RealTimeQuiz.Logic.Services.Interfaces;

public interface ISessionService
{
    Task<CreateSessionResultDto> CreateSessionAsync(
        CreateSessionRequest request,
        string ownerId,
        CancellationToken cancellationToken = default);

    Task<SessionLifecycleResultDto> OpenLobbyAsync(
        int sessionId,
        string ownerId,
        CancellationToken cancellationToken = default);

    Task<SessionLifecycleResultDto> StartSessionAsync(
        int sessionId,
        string ownerId,
        CancellationToken cancellationToken = default);

    Task<SessionLifecycleResultDto> CloseCurrentQuestionAsync(
        int sessionId,
        string ownerId,
        CancellationToken cancellationToken = default);

    Task<SessionLifecycleResultDto> AdvanceToNextQuestionAsync(
        int sessionId,
        string ownerId,
        CancellationToken cancellationToken = default);

    Task<SessionLifecycleResultDto> FinishSessionAsync(
        int sessionId,
        string ownerId,
        CancellationToken cancellationToken = default);

    Task<SessionLifecycleResultDto> CancelSessionAsync(
        int sessionId,
        string ownerId,
        CancellationToken cancellationToken = default);

    Task<SessionDetailsDto> GetSessionDetailsAsync(
        int sessionId,
        string ownerId,
        CancellationToken cancellationToken = default);
}