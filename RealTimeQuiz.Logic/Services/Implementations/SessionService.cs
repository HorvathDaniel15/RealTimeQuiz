using RealTimeQuiz.Logic.Contracts.Sessions.Requests;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.Model.Entities;
using RealTimeQuiz.Model.Enums;

namespace RealTimeQuiz.Logic.Services.Implementations;

public class SessionService : ISessionService
{
    public Task<CreateSessionResultDto> CreateSessionAsync(CreateSessionRequest request, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SessionLifecycleResultDto> OpenLobbyAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SessionLifecycleResultDto> StartSessionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SessionLifecycleResultDto> CloseCurrentQuestionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SessionLifecycleResultDto> AdvanceToNextQuestionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SessionLifecycleResultDto> FinishSessionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SessionLifecycleResultDto> CancelSessionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SessionDetailsDto> GetSessionDetailsAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private void ValidateOwnerId(string ownerId)
    {
        
    }

    private void ValidateSessionId(int session)
    {
        
    }
    
    private void ValidateCreateSessionRequest(CreateSessionRequest request)
    {
        
    }
    
    private Task LoadOwnedSessionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private Task LoadOwnedQuizAsync(int quizId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private void GetOrderedQuestions(Quiz quiz)
    {
        
    }

    private void GetFirstQuestion(Quiz quiz)
    {
        
    }
    
    private void GetNextQuestion(Quiz quiz, int currentQuestionId)
    {
        
    }

    private void EnsureState(QuizSession session, params SessionState[] allowedStates)
    {
        
    }
    
    private Task GenerateUniqueJoinPinAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private static CreateSessionResultDto MapToCreateSessionResultDto(QuizSession quizSession)
    {
        throw new NotImplementedException();
    }

    private static SessionLifecycleResultDto MapToSessionLifecycleResultDto(QuizSession quizSession)
    {
        throw new NotImplementedException();
    }

    private static SessionDetailsDto MapToSessionDetailsDto(QuizSession quizSession)
    {
        throw new NotImplementedException();
    }
}