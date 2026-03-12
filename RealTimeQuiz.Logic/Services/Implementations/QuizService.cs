using RealTimeQuiz.Data.Interfaces;
using RealTimeQuiz.Logic.Contracts.Quizzes.Requests;
using RealTimeQuiz.Logic.Contracts.Quizzes.Responses;
using RealTimeQuiz.Logic.Exceptions;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.Model.Entities;

namespace RealTimeQuiz.Logic.Services.Implementations;

public class QuizService : IQuizService
{
    private const int QuizTitleMaxLength = 200;
    private const int QuizDescriptionMaxLength = 2000;
    private const int QuestionTextMaxLength = 1000;
    private const int QuestionImageUrlMaxLength = 1000;
    private const int OptionTextMaxLength = 300;

    private const int MinQuestionCount = 1;
    private const int MinOptionCount = 2;
    private const int RequiredCorrectOptionCount = 1;
    private const int MinTimeLimitSeconds = 5;
    private const int MaxTimeLimitSeconds = 300;
    
    private readonly IQuizRepository _quizRepository;

    public QuizService(IQuizRepository quizRepository)
    {
        _quizRepository = quizRepository;
    }
    
    public async Task<IReadOnlyList<QuizListItemDto>> GetOwnQuizzesAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);
        
        var quizzes = await _quizRepository.GetByOwnerAsync(ownerId, cancellationToken);

        return quizzes
            .Select(MapToQuizListItemDto)
            .ToList();
    }

    public async Task<QuizDetailsDto> GetQuizDetailsAsync(int quizId, string ownerId,
        CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);

        if (quizId <= 0)
        {
            throw new BusinessValidationException("The quiz ID must be a positive number.");
        }
        
        var quiz = await _quizRepository.GetDetailedByIdAsync(quizId, cancellationToken);

        if (quiz is null)
        {
            throw new EntityNotFoundException("The specified quiz cannot be found.");
        }

        if (!string.Equals(quiz.OwnerId, ownerId, StringComparison.Ordinal))
        {
            throw new ForbiddenOperationException("The specified quiz does not belong to the current user.");
        }

        return MapToQuizDetailsDto(quiz);
    }

    public async Task<CreateQuizResultDto> CreateQuizAsync(CreateQuizRequest request, string ownerId, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);
        ValidateCreateQuizRequest(request);

        var quiz = new Quiz
        {
            Title = request.Title.Trim(),
            Description = NormalizeNullableText(request.Description),
            OwnerId = ownerId,
            IsPublished = false,
            CreatedUtc = DateTime.UtcNow,
            Questions = request.Questions
                .Select((question, questionIndex) => new QuizQuestion
                {
                    Text = question.Text.Trim(),
                    OrderIndex = questionIndex,
                    TimeLimitSeconds = question.TimeLimitSeconds,
                    ImageUrl = NormalizeNullableText(question.ImageUrl),
                    Options = question.Options
                        .Select((option, optionIndex) => new QuestionOption
                        {
                            Text = option.Text.Trim(),
                            OrderIndex = optionIndex,
                            IsCorrect = option.IsCorrect
                        })
                        .ToList()
                })
                .ToList()
        };
        
        await _quizRepository.AddAsync(quiz, cancellationToken);

        return new CreateQuizResultDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            IsPublished = quiz.IsPublished,
            CreatedUtc = quiz.CreatedUtc,
        };
    }
    
    private static void ValidateOwnerId(string ownerId)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            throw new BusinessValidationException("The current user ID is mandatory.");
        }
    }

    private static string? NormalizeNullableText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static QuizListItemDto MapToQuizListItemDto(Quiz quiz)
    {
        return new QuizListItemDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            IsPublished =  quiz.IsPublished,
            CreatedUtc = quiz.CreatedUtc
        };
    }

    private static QuizDetailsDto MapToQuizDetailsDto(Quiz quiz)
    {
        return new QuizDetailsDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            IsPublished = quiz.IsPublished,
            CreatedUtc = quiz.CreatedUtc,
            UpdatedAtUtc =  quiz.UpdatedAtUtc,
            Questions = quiz.Questions
                .OrderBy(q=> q.OrderIndex)
                .Select(MapToQuizQuestionDto)
                .ToList()
        };
    }
    
    private static QuizQuestionDto MapToQuizQuestionDto(QuizQuestion question)
    {
        return new QuizQuestionDto
        {
            Id = question.Id,
            Text = question.Text,
            ImageUrl = question.ImageUrl,
            OrderIndex =  question.OrderIndex,
            TimeLimitSeconds = question.TimeLimitSeconds,
            Options = question.Options
                .OrderBy(o => o.OrderIndex)
                .Select(MapToQuestionOptionDto)
                .ToList()
        };
    }
    
    private static QuestionOptionDto MapToQuestionOptionDto(QuestionOption option)
    {
        return new QuestionOptionDto
        {
            Id = option.Id,
            Text = option.Text,
            OrderIndex = option.OrderIndex,
            IsCorrect = option.IsCorrect
        };
    }
    
    private void ValidateCreateQuizRequest(CreateQuizRequest request)
    {
        if (request is null)
        {
            throw new BusinessValidationException("The request can't be null.");
        }

        var errors = new List<string>();

        ValidateQuizLevelRules(request, errors);

        if (request.Questions is not null)
        {
            for (var questionIndex = 0; questionIndex < request.Questions.Count; questionIndex++)
            {
                ValidateQuestionRules(request.Questions[questionIndex], questionIndex, errors);
            }
        }
        
        if (errors.Count > 0)
        {
            throw new BusinessValidationException(errors);
        }
    }
    
    private void ValidateQuizLevelRules(CreateQuizRequest request, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add("The quiz title is required.");
        }
        else if (request.Title.Trim().Length > QuizTitleMaxLength)
        {
            errors.Add($"The quiz title can be up to {QuizTitleMaxLength} characters long.");
        }

        if (!string.IsNullOrWhiteSpace(request.Description) &&
            request.Description.Trim().Length > QuizDescriptionMaxLength)
        {
            errors.Add($"The quiz description can be up to {QuizDescriptionMaxLength} characters long.");
        }

        if (request.Questions is null || request.Questions.Count < MinQuestionCount)
        {
            errors.Add("The quiz must contain at least 1 question.");
        }
    }
    
    private void ValidateQuestionRules(
        CreateQuizQuestionRequest question,
        int questionIndex,
        List<string> errors)
    {
        var questionNumber = questionIndex + 1;

        if (question is null)
        {
            errors.Add($"Question #{questionNumber} is missing.");
            return;
        }

        if (string.IsNullOrWhiteSpace(question.Text))
        {
            errors.Add($"Question #{questionNumber} text is required.");
        }
        else if (question.Text.Trim().Length > QuestionTextMaxLength)
        {
            errors.Add($"Question #{questionNumber} text can be up to {QuestionTextMaxLength} characters long.");
        }

        if (!string.IsNullOrWhiteSpace(question.ImageUrl) &&
            question.ImageUrl.Trim().Length > QuestionImageUrlMaxLength)
        {
            errors.Add($"Question #{questionNumber} image URL can be up to {QuestionImageUrlMaxLength} characters long.");
        }

        if (question.TimeLimitSeconds < MinTimeLimitSeconds || question.TimeLimitSeconds > MaxTimeLimitSeconds)
        {
            errors.Add(
                $"Question #{questionNumber} time limit must be between {MinTimeLimitSeconds} and {MaxTimeLimitSeconds} seconds.");
        }

        if (question.Options is null || question.Options.Count < MinOptionCount)
        {
            errors.Add($"Question #{questionNumber} must contain at least {MinOptionCount} answer options.");
            return;
        }

        ValidateOptionRules(question.Options, questionNumber, errors);
    }
    
    private void ValidateOptionRules(
        List<CreateQuestionOptionRequest> options,
        int questionNumber,
        List<string> errors)
    {
        var correctOptionCount = 0;

        for (var optionIndex = 0; optionIndex < options.Count; optionIndex++)
        {
            var option = options[optionIndex];
            var optionNumber = optionIndex + 1;

            if (option is null)
            {
                errors.Add($"Option #{optionNumber} for question #{questionNumber} is missing.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(option.Text))
            {
                errors.Add($"Text for option #{optionNumber} of question #{questionNumber} is required.");
            }
            else if (option.Text.Trim().Length > OptionTextMaxLength)
            {
                errors.Add(
                    $"Option #{optionNumber} for question #{questionNumber} can be up to {OptionTextMaxLength} characters long.");
            }

            if (option.IsCorrect)
            {
                correctOptionCount++;
            }
        }

        if (correctOptionCount != RequiredCorrectOptionCount)
        {
            errors.Add(
                $"Question #{questionNumber} must have exactly {RequiredCorrectOptionCount} correct option.");
        }
    }
}