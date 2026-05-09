using RealTimeQuiz.Data.Interfaces;
using RealTimeQuiz.Logic.Contracts.Sessions.Requests;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;
using RealTimeQuiz.Logic.Exceptions;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.Model.Entities;
using RealTimeQuiz.Model.Enums;

namespace RealTimeQuiz.Logic.Services.Implementations;

public class ParticipantSessionService : IParticipantSessionService
{
    private const int JoinPinLength = 6;
    private const int DisplayNameMaxLength = 100;

    private readonly IQuizSessionRepository _quizSessionRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly ISessionAnswerRepository _sessionAnswerRepository;

    public ParticipantSessionService(
        IQuizSessionRepository quizSessionRepository,
        IParticipantRepository participantRepository,
        ISessionAnswerRepository sessionAnswerRepository)
    {
        _quizSessionRepository = quizSessionRepository;
        _participantRepository = participantRepository;
        _sessionAnswerRepository = sessionAnswerRepository;
    }

    public async Task<JoinSessionResultDto> JoinByPinAsync(JoinSessionByPinRequest request, string? userId,
        CancellationToken cancellationToken = default)
    {
        ValidateJoinRequest(request);

        var normalizedDisplayName = NormalizeDisplayName(request.DisplayName);
        var session = await LoadSessionByPinAsync(request.JoinPin, cancellationToken);

        EnsureJoinableState(session);
        await EnsureDisplayNameIsUniqueAsync(session.Id, normalizedDisplayName, cancellationToken);

        var participant = new SessionParticipant
        {
            QuizSessionId = session.Id,
            DisplayName = normalizedDisplayName,
            UserId = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim(),
            JoinedAtUtc = DateTime.UtcNow,
            TotalScore = 0,
        };

        var wasSaved = await _participantRepository.TryAddAsync(participant, cancellationToken);
        if (!wasSaved)
        {
            throw new BusinessValidationException("The selected display name is already in use in this session.");
        }

        return MapToJoinSessionResultDto(participant, session);
    }

    public async Task<ParticipantCurrentQuestionDto> GetCurrentQuestionForParticipantAsync(GetCurrentQuestionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new BusinessValidationException("The current question request cannot be null.");
        }

        var (_, session) = await LoadParticipantWithSessionGraphAsync(request.ParticipantId, cancellationToken);

        EnsureQuestionOpenState(session);
        var currentQuestion = EnsureCurrentQuestionConsistency(session, session.CurrentQuestionId ?? 0);

        if (currentQuestion.Options is null)
        {
            throw new BusinessValidationException("The current question options are not loaded.");
        }

        return MapToParticipantCurrentQuestionDto(session, currentQuestion);
    }

    public async Task<SubmitAnswerResultDto> SubmitAnswerAsync(SubmitAnswerRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateSubmitAnswerRequest(request);

        var (participant, session) = await LoadParticipantWithSessionGraphAsync(request.ParticipantId, cancellationToken);

        EnsureQuestionOpenState(session);
        var currentQuestion = EnsureCurrentQuestionConsistency(session, request.QuestionId);

        if (session.QuestionOpenedAtUtc.HasValue && currentQuestion.TimeLimitSeconds > 0)
        {
            var timeLimit = session.QuestionOpenedAtUtc.Value.AddSeconds(currentQuestion.TimeLimitSeconds);
            if (DateTime.UtcNow > timeLimit)
            {
                throw new BusinessValidationException("The time limit for this question has expired.");
            }
        }

        var selectedOption = FindOptionInCurrentQuestion(currentQuestion, request.OptionId);

        var existingAnswer = await _sessionAnswerRepository
            .GetByParticipantAndQuestionAsync(participant.Id, currentQuestion.Id, cancellationToken);
        if (existingAnswer is not null)
        {
            throw new BusinessValidationException("You have already submitted an answer for this question.");
        }

        var answer = new SessionAnswer
        {
            QuizSessionId = session.Id,
            SessionParticipantId = participant.Id,
            QuizQuestionId = currentQuestion.Id,
            QuestionOptionId = selectedOption.Id,
            SubmittedAtUtc = DateTime.UtcNow,
            Status = SubmissionStatus.Accepted,
            IsCorrect = selectedOption.IsCorrect,
            AwardedPoints = 0,
        };

        var wasSaved = await _sessionAnswerRepository.TryAddAsync(answer, cancellationToken);
        if (!wasSaved)
        {
            throw new BusinessValidationException("You have already submitted an answer for this question.");
        }

        return MapToSubmitAnswerResultDto(answer);
    }

    public async Task<SubmitAnswerResultDto> GetAnswerResultAsync(
        GetParticipantAnswerResultRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateGetAnswerResultRequest(request);

        var (_, session) = await LoadParticipantWithSessionGraphAsync(request.ParticipantId, cancellationToken);
        EnsureResultReadableState(session);
        EnsureQuestionBelongsToSession(session, request.QuestionId);

        var answer = await _sessionAnswerRepository
            .GetByParticipantAndQuestionAsync(request.ParticipantId, request.QuestionId, cancellationToken);

        if (answer is null)
        {
            throw new EntityNotFoundException("No submitted answer found for this participant and question.");
        }

        return MapToSubmitAnswerResultDto(answer);
    }

    private static void ValidateJoinRequest(JoinSessionByPinRequest request)
    {
        if (request is null)
        {
            throw new BusinessValidationException("The join request cannot be null.");
        }

        var errors = new List<string>();

        var joinPin = request.JoinPin?.Trim();
        if (string.IsNullOrWhiteSpace(joinPin))
        {
            errors.Add("Join PIN is required.");
        }
        else if (joinPin.Length != JoinPinLength)
        {
            errors.Add($"Join PIN must be exactly {JoinPinLength} characters long.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors.Add("Display name is required.");
        }
        else if (request.DisplayName.Trim().Length > DisplayNameMaxLength)
        {
            errors.Add($"Display name can be up to {DisplayNameMaxLength} characters long.");
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationException(errors);
        }
    }

    private static void ValidateSubmitAnswerRequest(SubmitAnswerRequest request)
    {
        if (request is null)
        {
            throw new BusinessValidationException("The submit answer request cannot be null.");
        }

        var errors = new List<string>();
        if (request.ParticipantId <= 0)
        {
            errors.Add("Participant ID must be a positive number.");
        }

        if (request.QuestionId <= 0)
        {
            errors.Add("Question ID must be a positive number.");
        }

        if (request.OptionId <= 0)
        {
            errors.Add("Option ID must be a positive number.");
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationException(errors);
        }
    }

    private static void ValidateGetAnswerResultRequest(GetParticipantAnswerResultRequest request)
    {
        if (request is null)
        {
            throw new BusinessValidationException("The answer result request cannot be null.");
        }

        var errors = new List<string>();

        if (request.ParticipantId <= 0)
        {
            errors.Add("Participant ID must be a positive number.");
        }

        if (request.QuestionId <= 0)
        {
            errors.Add("Question ID must be a positive number.");
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationException(errors);
        }
    }

    private async Task<QuizSession> LoadSessionByPinAsync(string joinPin, CancellationToken cancellationToken)
    {
        var normalizedPin = joinPin?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedPin))
        {
            throw new BusinessValidationException("Join PIN is required.");
        }

        var session = await _quizSessionRepository.GetByPinAsync(normalizedPin, cancellationToken);
        if (session is null)
        {
            throw new EntityNotFoundException("No active session was found with the provided join PIN.");
        }

        return session;
    }

    private async Task<(SessionParticipant Participant, QuizSession Session)> LoadParticipantWithSessionGraphAsync(
        int participantId,
        CancellationToken cancellationToken)
    {
        if (participantId <= 0)
        {
            throw new BusinessValidationException("Participant ID must be a positive number.");
        }

        var participant = await _participantRepository.GetByIdAsync(participantId, cancellationToken);
        if (participant is null)
        {
            throw new EntityNotFoundException($"No participant found with ID {participantId}.");
        }

        var session = await _quizSessionRepository.GetDetailedByIdAsync(participant.QuizSessionId, cancellationToken);
        if (session is null)
        {
            throw new EntityNotFoundException(
                $"No quiz session found with ID {participant.QuizSessionId} for participant {participantId}.");
        }

        if (session.Quiz is null || session.Quiz.Questions is null)
        {
            throw new BusinessValidationException("The loaded session is missing quiz graph data.");
        }

        participant.QuizSession = session;
        return (participant, session);
    }

    private static void EnsureJoinableState(QuizSession session)
    {
        if (session is null)
        {
            throw new BusinessValidationException("Session cannot be null.");
        }

        if (session.State != SessionState.Lobby)
        {
            throw new ForbiddenOperationException(
                $"Cannot join session in '{session.State}' state. Allowed state: '{SessionState.Lobby}'.");
        }
    }

    private static void EnsureQuestionOpenState(QuizSession session)
    {
        if (session is null)
        {
            throw new BusinessValidationException("Session cannot be null.");
        }

        if (session.State != SessionState.QuestionOpen)
        {
            throw new ForbiddenOperationException(
                $"Cannot submit or fetch question in '{session.State}' state. Allowed state: '{SessionState.QuestionOpen}'.");
        }
    }

    private static void EnsureResultReadableState(QuizSession session)
    {
        if (session is null)
        {
            throw new BusinessValidationException("Session cannot be null.");
        }

        if (session.State is SessionState.Draft or SessionState.Lobby)
        {
            throw new ForbiddenOperationException(
                $"Cannot fetch answer results in '{session.State}' state. Allowed states: '{SessionState.QuestionOpen}', '{SessionState.QuestionClosed}', '{SessionState.Finished}', '{SessionState.Canceled}'.");
        }
    }

    private static void EnsureQuestionBelongsToSession(QuizSession session, int questionId)
    {
        if (questionId <= 0)
        {
            throw new BusinessValidationException("Question ID must be a positive number.");
        }

        if (session.Quiz is null || session.Quiz.Questions is null)
        {
            throw new BusinessValidationException("The loaded session is missing quiz question data.");
        }

        if (!session.Quiz.Questions.Any(x => x.Id == questionId && x.QuizId == session.QuizId))
        {
            throw new BusinessValidationException("The requested question does not belong to the participant's session.");
        }
    }

    private static QuizQuestion EnsureCurrentQuestionConsistency(QuizSession session, int requestQuestionId)
    {
        if (session is null)
        {
            throw new BusinessValidationException("Session cannot be null.");
        }

        if (!session.CurrentQuestionId.HasValue)
        {
            throw new BusinessValidationException("No current question is selected for this session.");
        }

        if (requestQuestionId <= 0)
        {
            throw new BusinessValidationException("Question ID must be a positive number.");
        }

        if (session.Quiz is null || session.Quiz.Questions is null)
        {
            throw new BusinessValidationException("The loaded session is missing quiz question data.");
        }

        if (session.CurrentQuestionId.Value != requestQuestionId)
        {
            throw new BusinessValidationException(
                $"Submitted question ID {requestQuestionId} does not match current question ID {session.CurrentQuestionId.Value}.");
        }

        var currentQuestion = session.Quiz.Questions.FirstOrDefault(q => q.Id == session.CurrentQuestionId.Value);
        if (currentQuestion is null)
        {
            throw new BusinessValidationException("The current question is not part of the loaded quiz.");
        }

        if (currentQuestion.QuizId != session.QuizId)
        {
            throw new BusinessValidationException("The current question does not belong to the session quiz.");
        }

        return currentQuestion;
    }

    private static QuestionOption FindOptionInCurrentQuestion(QuizQuestion currentQuestion, int optionId)
    {
        if (optionId <= 0)
        {
            throw new BusinessValidationException("Option ID must be a positive number.");
        }

        if (currentQuestion is null)
        {
            throw new BusinessValidationException("The current question is not part of the loaded quiz.");
        }

        if (currentQuestion.Options is null)
        {
            throw new BusinessValidationException("The current question options are not loaded.");
        }

        var option = currentQuestion.Options.FirstOrDefault(x => x.Id == optionId);
        if (option is null)
        {
            throw new BusinessValidationException("The selected option is not part of the current question.");
        }

        if (option.QuizQuestionId != currentQuestion.Id)
        {
            throw new BusinessValidationException("The selected option does not belong to the current question.");
        }

        return option;
    }

    private async Task EnsureDisplayNameIsUniqueAsync(int sessionId, string normalizedDisplayName,
        CancellationToken cancellationToken)
    {
        if (sessionId <= 0)
        {
            throw new BusinessValidationException("Session ID must be a positive number.");
        }

        if (string.IsNullOrWhiteSpace(normalizedDisplayName))
        {
            throw new BusinessValidationException("Display name is required.");
        }

        var existingParticipant = await _participantRepository
            .GetBySessionAndNameAsync(sessionId, normalizedDisplayName, cancellationToken);

        if (existingParticipant is not null)
        {
            throw new BusinessValidationException("The selected display name is already in use in this session.");
        }
    }

    private static string NormalizeDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new BusinessValidationException("Display name is required.");
        }

        var normalized = displayName.Trim();
        if (normalized.Length > DisplayNameMaxLength)
        {
            throw new BusinessValidationException($"Display name can be up to {DisplayNameMaxLength} characters long.");
        }

        return normalized;
    }

    private static JoinSessionResultDto MapToJoinSessionResultDto(SessionParticipant participant, QuizSession session)
    {
        return new JoinSessionResultDto
        {
            ParticipantId = participant.Id,
            SessionId = session.Id,
            JoinPin = session.JoinPin,
            DisplayName = participant.DisplayName,
            SessionState = session.State,
            JoinedAtUtc = participant.JoinedAtUtc,
        };
    }

    private static ParticipantCurrentQuestionDto MapToParticipantCurrentQuestionDto(QuizSession session, QuizQuestion question)
    {
        if (!session.QuestionOpenedAtUtc.HasValue)
        {
            throw new BusinessValidationException("Question opened timestamp is missing for a question-open session.");
        }

        return new ParticipantCurrentQuestionDto
        {
            SessionId = session.Id,
            QuestionId = question.Id,
            OrderIndex = question.OrderIndex,
            Text = question.Text,
            ImageUrl = question.ImageUrl,
            TimeLimitSeconds = question.TimeLimitSeconds,
            OpenedAtUtc = session.QuestionOpenedAtUtc.Value,
            Options = question.Options
                .OrderBy(x => x.OrderIndex)
                .ThenBy(x => x.Id)
                .Select(x => new ParticipantQuestionOptionDto
                {
                    Id = x.Id,
                    Text = x.Text,
                    OrderIndex = x.OrderIndex,
                })
                .ToList(),
        };
    }

    private static SubmitAnswerResultDto MapToSubmitAnswerResultDto(SessionAnswer answer)
    {
        return new SubmitAnswerResultDto
        {
            SessionId = answer.QuizSessionId,
            ParticipantId = answer.SessionParticipantId,
            QuestionId = answer.QuizQuestionId,
            OptionId = answer.QuestionOptionId,
            Status = answer.Status,
            IsCorrect = answer.IsCorrect,
            AwardedPoints = answer.AwardedPoints,
            SubmittedAtUtc = answer.SubmittedAtUtc,
        };
    }
}