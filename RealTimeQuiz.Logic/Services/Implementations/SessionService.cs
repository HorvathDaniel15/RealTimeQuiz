using RealTimeQuiz.Data.Interfaces;
using RealTimeQuiz.Logic.Contracts.Sessions.Requests;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;
using RealTimeQuiz.Logic.Exceptions;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.Model.Entities;
using RealTimeQuiz.Model.Enums;
using System.Security.Cryptography;

namespace RealTimeQuiz.Logic.Services.Implementations;

public class SessionService : ISessionService
{
    private const int JoinPinLength = 6;
    private const int MaxJoinPinGenerationAttempts = 100;

    private readonly IQuizRepository _quizRepository;
    private readonly IQuizSessionRepository _quizSessionRepository;

    public SessionService(IQuizRepository quizRepository, IQuizSessionRepository quizSessionRepository)
    {
        _quizRepository = quizRepository;
        _quizSessionRepository = quizSessionRepository;
    }
    
    public async Task<CreateSessionResultDto> CreateSessionAsync(CreateSessionRequest request, string ownerId, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);
        ValidateCreateSessionRequest(request);

        var quiz = await LoadOwnedQuizAsync(request.QuizId, ownerId, cancellationToken);

        if (GetFirstQuestion(quiz) is null)
        {
            throw new BusinessValidationException("The quiz must contain at least one question before creating a session.");
        }

        for (var attempt = 0; attempt < MaxJoinPinGenerationAttempts; attempt++)
        {
            var session = new QuizSession
            {
                QuizId = quiz.Id,
                JoinPin = GenerateJoinPin(),
                State = SessionState.Draft,
                CurrentQuestionId = null,
                CreatedAtUtc = DateTime.UtcNow,
                StartedAtUtc = null,
                QuestionOpenedAtUtc = null,
                QuestionClosedAtUtc = null,
                FinishedAtUtc = null,
            };

            if (await _quizSessionRepository.TryAddAsync(session, cancellationToken))
            {
                return MapToCreateSessionResultDto(session);
            }
        }

        throw new BusinessValidationException("Unable to generate a unique join PIN. Please try again.");
    }

    public async Task<SessionLifecycleResultDto> OpenLobbyAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        var session = await LoadOwnedSessionAsync(sessionId, ownerId, cancellationToken);
        EnsureState(session, SessionState.Draft);

        session.State = SessionState.Lobby;

        await _quizSessionRepository.UpdateAsync(session, cancellationToken);
        return MapToSessionLifecycleResultDto(session);
    }

    public async Task<SessionLifecycleResultDto> StartSessionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        var session = await LoadOwnedSessionAsync(sessionId, ownerId, cancellationToken);
        EnsureState(session, SessionState.Lobby);

        var firstQuestion = GetFirstQuestion(session.Quiz)
                            ?? throw new BusinessValidationException(
                                "The quiz must contain at least one question before starting the session.");

        var nowUtc = DateTime.UtcNow;

        session.State = SessionState.QuestionOpen;
        session.CurrentQuestionId = firstQuestion.Id;
        session.StartedAtUtc ??= nowUtc;
        session.QuestionOpenedAtUtc = nowUtc;
        session.QuestionClosedAtUtc = null;

        await _quizSessionRepository.UpdateAsync(session, cancellationToken);
        return MapToSessionLifecycleResultDto(session);
    }

    public async Task<SessionLifecycleResultDto> CloseCurrentQuestionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        var session = await LoadOwnedSessionAsync(sessionId, ownerId, cancellationToken);
        EnsureState(session, SessionState.QuestionOpen);

        if (!session.CurrentQuestionId.HasValue)
        {
            throw new BusinessValidationException("Cannot close question because no current question is selected.");
        }

        session.State = SessionState.QuestionClosed;
        session.QuestionClosedAtUtc = DateTime.UtcNow;

        await _quizSessionRepository.UpdateAsync(session, cancellationToken);
        return MapToSessionLifecycleResultDto(session);
    }

    public async Task<SessionLifecycleResultDto> AdvanceToNextQuestionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        var session = await LoadOwnedSessionAsync(sessionId, ownerId, cancellationToken);
        EnsureState(session, SessionState.QuestionClosed);

        if (!session.CurrentQuestionId.HasValue)
        {
            throw new BusinessValidationException("Cannot advance because no current question is selected.");
        }

        var nextQuestion = GetNextQuestion(session.Quiz, session.CurrentQuestionId.Value);
        var nowUtc = DateTime.UtcNow;

        if (nextQuestion is null)
        {
            session.State = SessionState.Finished;
            session.CurrentQuestionId = null;
            session.FinishedAtUtc = nowUtc;
        }
        else
        {
            session.State = SessionState.QuestionOpen;
            session.CurrentQuestionId = nextQuestion.Id;
            session.QuestionOpenedAtUtc = nowUtc;
            session.QuestionClosedAtUtc = null;
        }

        await _quizSessionRepository.UpdateAsync(session, cancellationToken);
        return MapToSessionLifecycleResultDto(session);
    }

    public async Task<SessionLifecycleResultDto> FinishSessionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        var session = await LoadOwnedSessionAsync(sessionId, ownerId, cancellationToken);
        EnsureState(session, SessionState.Lobby, SessionState.QuestionOpen, SessionState.QuestionClosed);

        var nowUtc = DateTime.UtcNow;
        if (session.State == SessionState.QuestionOpen)
        {
            session.QuestionClosedAtUtc ??= nowUtc;
        }

        session.State = SessionState.Finished;
        session.CurrentQuestionId = null;
        session.FinishedAtUtc = nowUtc;

        await _quizSessionRepository.UpdateAsync(session, cancellationToken);
        return MapToSessionLifecycleResultDto(session);
    }

    public async Task<SessionLifecycleResultDto> CancelSessionAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        var session = await LoadOwnedSessionAsync(sessionId, ownerId, cancellationToken);
        EnsureState(session, SessionState.Draft, SessionState.Lobby, SessionState.QuestionOpen, SessionState.QuestionClosed);

        var nowUtc = DateTime.UtcNow;
        if (session.State == SessionState.QuestionOpen)
        {
            session.QuestionClosedAtUtc ??= nowUtc;
        }

        session.State = SessionState.Canceled;
        session.CurrentQuestionId = null;
        session.FinishedAtUtc = nowUtc;

        await _quizSessionRepository.UpdateAsync(session, cancellationToken);
        return MapToSessionLifecycleResultDto(session);
    }

    public async Task<SessionDetailsDto> GetSessionDetailsAsync(int sessionId, string ownerId, CancellationToken cancellationToken = default)
    {
        var session = await LoadOwnedSessionAsync(sessionId, ownerId, cancellationToken);
        return MapToSessionDetailsDto(session);
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
            throw new BusinessValidationException("The quiz questions are not loaded.");
        }

        if (quiz.Questions.Any(q => q.QuizId != quiz.Id))
        {
            throw new BusinessValidationException("The loaded quiz contains questions from another quiz.");
        }

        if (quiz.Questions.Any(q => q.OrderIndex < 0))
        {
            throw new BusinessValidationException("Question order index cannot be negative.");
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
        return GetOrderedQuestions(quiz).FirstOrDefault();
    }
    
    private QuizQuestion? GetNextQuestion(Quiz quiz, int currentQuestionId)
    {
        if (currentQuestionId <= 0)
        {
            throw new BusinessValidationException("The current question ID must be a positive number.");
        }

        var orderedQuestions = GetOrderedQuestions(quiz);
        var currentIndex = orderedQuestions
            .Select((question, index) => new { question.Id, index })
            .FirstOrDefault(x => x.Id == currentQuestionId)?.index;

        if (currentIndex is null)
        {
            throw new BusinessValidationException("The current question is not part of this quiz.");
        }

        var nextIndex = currentIndex.Value + 1;
        return nextIndex < orderedQuestions.Count
            ? orderedQuestions[nextIndex]
            : null;
    }

    private void EnsureState(QuizSession session, params SessionState[] allowedStates)
    {
        if (session is null)
        {
            throw new BusinessValidationException("The session cannot be null.");
        }

        if (allowedStates is null || allowedStates.Length == 0)
        {
            throw new BusinessValidationException("No allowed states were configured for this transition.");
        }

        if (!allowedStates.Contains(session.State))
        {
            var allowed = string.Join(", ", allowedStates.Select(x => x.ToString()));
            throw new ForbiddenOperationException(
                $"Invalid session transition from '{session.State}'. Allowed states: {allowed}.");
        }
    }
    
    private static string GenerateJoinPin()
    {
        // Uniqueness is guaranteed by the DB unique index and handled by TryAddAsync retries.
        var randomValue = RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, JoinPinLength));
        return randomValue.ToString($"D{JoinPinLength}");
    }

    private static CreateSessionResultDto MapToCreateSessionResultDto(QuizSession quizSession)
    {
        return new CreateSessionResultDto
        {
            Id = quizSession.Id,
            QuizId = quizSession.QuizId,
            JoinPin = quizSession.JoinPin,
            State = quizSession.State,
            CreatedAtUtc = quizSession.CreatedAtUtc,
        };
    }

    private static SessionLifecycleResultDto MapToSessionLifecycleResultDto(QuizSession quizSession)
    {
        EnsureSessionGraphLoadedForMapping(quizSession);

        var currentQuestion = quizSession.Quiz.Questions
            .FirstOrDefault(q => q.Id == quizSession.CurrentQuestionId);

        return new SessionLifecycleResultDto
        {
            Id = quizSession.Id,
            QuizId = quizSession.QuizId,
            JoinPin = quizSession.JoinPin,
            State = quizSession.State,
            CurrentQuestionId = quizSession.CurrentQuestionId,
            StartedAtUtc = quizSession.StartedAtUtc,
            QuestionOpenedAtUtc = quizSession.QuestionOpenedAtUtc,
            QuestionClosedAtUtc = quizSession.QuestionClosedAtUtc,
            FinishedAtUtc = quizSession.FinishedAtUtc,
            CurrentQuestionOrderIndex = currentQuestion?.OrderIndex,
            CurrentQuestionText = currentQuestion?.Text,
        };
    }

    private static SessionDetailsDto MapToSessionDetailsDto(QuizSession quizSession)
    {
        EnsureSessionGraphLoadedForMapping(quizSession);

        var currentQuestion = quizSession.Quiz.Questions
            .FirstOrDefault(q => q.Id == quizSession.CurrentQuestionId);

        return new SessionDetailsDto
        {
            Id = quizSession.Id,
            QuizId = quizSession.QuizId,
            QuizTitle = quizSession.Quiz.Title,
            JoinPin = quizSession.JoinPin,
            State = quizSession.State,
            CreatedAtUtc = quizSession.CreatedAtUtc,
            StartedAtUtc = quizSession.StartedAtUtc,
            QuestionOpenedAtUtc = quizSession.QuestionOpenedAtUtc,
            QuestionClosedAtUtc = quizSession.QuestionClosedAtUtc,
            FinishedAtUtc = quizSession.FinishedAtUtc,
            CurrentQuestionId = quizSession.CurrentQuestionId,
            ParticipantCount = quizSession.Participants.Count,
            CurrentQuestion = currentQuestion is null
                ? null
                : new SessionCurrentQuestionDto
                {
                    Id = currentQuestion.Id,
                    OrderIndex = currentQuestion.OrderIndex,
                    Text = currentQuestion.Text,
                    TimeLimitSeconds = currentQuestion.TimeLimitSeconds,
                    ImageUrl = currentQuestion.ImageUrl,
                },
        };
    }

    private static void EnsureSessionGraphLoadedForMapping(QuizSession quizSession)
    {
        if (quizSession.Quiz is null)
        {
            throw new BusinessValidationException("The loaded session is missing quiz data for mapping.");
        }

        if (quizSession.Quiz.Questions is null)
        {
            throw new BusinessValidationException("The loaded session is missing quiz questions for mapping.");
        }

        if (quizSession.Participants is null)
        {
            throw new BusinessValidationException("The loaded session is missing participant data for mapping.");
        }
    }
}