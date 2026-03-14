using RealTimeQuiz.Data.Interfaces;
using RealTimeQuiz.Logic.Contracts.Sessions.Requests;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;
using RealTimeQuiz.Logic.Exceptions;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.Model.Entities;
using RealTimeQuiz.Model.Enums;

namespace RealTimeQuiz.Logic.Services.Implementations;

public class SessionService : ISessionService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizSessionRepository _quizSessionRepository;

    public SessionService(IQuizRepository quizRepository, IQuizSessionRepository quizSessionRepository)
    {
        _quizRepository = quizRepository;
        _quizSessionRepository = quizSessionRepository;
    }
    
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
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            throw new BusinessValidationException("The current user ID is mandatory.");
        }
    }

    private void ValidateSessionId(int sessionId)
    {
        if (sessionId <= 0)
        {
            throw new BusinessValidationException("The session ID must be a positive number.");
        }
    }
    
    private void ValidateCreateSessionRequest(CreateSessionRequest request)
    {
        if (request is null)
        {
            throw new BusinessValidationException("The create session request cannot be null.");
        }
        
        if (request.QuizId <= 0)
        {
            throw new BusinessValidationException("The quiz ID must be a positive number.");
        }
        
    }
    
    private Task<QuizSession> LoadOwnedSessionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private Task<Quiz> LoadOwnedQuizAsync(int quizId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private IReadOnlyList<QuizQuestion> GetOrderedQuestions(Quiz quiz)
    {
        throw new NotImplementedException();
    }

    private QuizQuestion? GetFirstQuestion(Quiz quiz)
    {
        throw new NotImplementedException();
    }
    
    private QuizQuestion? GetNextQuestion(Quiz quiz, int currentQuestionId)
    {
        throw new NotImplementedException();
    }

    private void EnsureState(QuizSession session, params SessionState[] allowedStates)
    {
        
    }
    
    private Task<string> GenerateUniqueJoinPinAsync(CancellationToken cancellationToken = default)
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