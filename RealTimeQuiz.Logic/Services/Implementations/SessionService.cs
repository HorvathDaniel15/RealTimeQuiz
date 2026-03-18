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
    
    public async Task<CreateSessionResultDto> CreateSessionAsync(CreateSessionRequest request, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<SessionLifecycleResultDto> OpenLobbyAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<SessionLifecycleResultDto> StartSessionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<SessionLifecycleResultDto> CloseCurrentQuestionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<SessionLifecycleResultDto> AdvanceToNextQuestionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<SessionLifecycleResultDto> FinishSessionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<SessionLifecycleResultDto> CancelSessionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<SessionDetailsDto> GetSessionDetailsAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
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

    private void ValidateQuizId(int quizId)
    {
        if (quizId <= 0)
        {
            throw new BusinessValidationException("The quiz ID must be a positive number.");
        }
    }
    
    private void ValidateCreateSessionRequest(CreateSessionRequest request)
    {
        if (request is null)
        {
            throw new BusinessValidationException("The create session request cannot be null.");
        }
        
        ValidateQuizId(request.QuizId);
        
    }
    
    private async Task<QuizSession> LoadOwnedSessionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);
        ValidateSessionId(sessionId);
        
        var session = await _quizSessionRepository.GetDetailedByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            throw new EntityNotFoundException($"No quiz session found with ID {sessionId}.");
        }
        
        if (session.Quiz is null || string.IsNullOrWhiteSpace(session.Quiz.OwnerId))
        {
            throw new BusinessValidationException("The loaded session is missing quiz ownership data.");
        }

        if (!string.Equals(session.Quiz.OwnerId, ownerId, StringComparison.Ordinal))
        {
            throw new ForbiddenOperationException("The current user is not the owner of this quiz session.");
        }

        return session;
    }

    private async Task<Quiz> LoadOwnedQuizAsync(int quizId, string ownerId, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);
        ValidateQuizId(quizId);
        
        var quiz = await _quizRepository.GetDetailedByIdAsync(quizId, cancellationToken);
        if (quiz is null)
        {
            throw new EntityNotFoundException($"No quiz found with ID {quizId}.");
        }

        if (string.IsNullOrWhiteSpace(quiz.OwnerId))
        {
            throw new BusinessValidationException("The loaded quiz is missing ownership data.");
        }

        if (!string.Equals(quiz.OwnerId, ownerId, StringComparison.Ordinal))
        {
            throw new ForbiddenOperationException("The current user is not the owner of this quiz.");
        }
        
        return quiz;
    }

    private IReadOnlyList<QuizQuestion> GetOrderedQuestions(Quiz quiz)
    {
        if (quiz is null)
        {
            throw new BusinessValidationException("The quiz cannot be null.");
        }

        if (quiz.Questions is null)
        {
            throw new BusinessValidationException("The quiz question are not loaded.");
        }

        if (quiz.Questions.Any(q=> q.QuizId == quiz.Id))
        {
            throw new BusinessValidationException("The loaded quiz contains questions from another quiz.");
        }

        var duplicateOrderIndex = quiz.Questions
            .GroupBy(q => q.OrderIndex)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateOrderIndex is not null)
        {
            throw new BusinessValidationException(
                $"Duplicate question order index detected: {duplicateOrderIndex.Key}.");
        }

        return quiz.Questions
            .OrderBy(q => q.OrderIndex)
            .ThenBy(q => q.Id)
            .ToList();
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
    
    private async Task<string> GenerateUniqueJoinPinAsync(CancellationToken cancellationToken = default)
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