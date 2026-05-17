using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RealTimeQuiz.Logic.Contracts.Auth.Requests;
using RealTimeQuiz.Logic.Contracts.Auth.Responses;
using RealTimeQuiz.Logic.Contracts.Quizzes.Responses;
using RealTimeQuiz.WebAPI.Contracts.Requests.Admin;

namespace RealTimeQuiz.Tests.Integration;

public class QuizIntegrationTests : BaseIntegrationTest
{
    public QuizIntegrationTests(RealTimeQuizWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateQuiz_WithValidData_ShouldReturnCreatedQuiz()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("createquiz");
        var createRequest = new CreateQuizApiRequest
        {
            Title = "Integrációs Teszt Kvíz",
            Description = "Ez egy automatikus teszt által generált kvíz.",
            Questions = new List<CreateQuizQuestionApiRequest>
            {
                new CreateQuizQuestionApiRequest
                {
                    Text = "Melyik a legjobb programozási nyelv?",
                    TimeLimitSeconds = 20,
                    Options = new List<CreateQuestionOptionApiRequest>
                    {
                        new CreateQuestionOptionApiRequest { Text = "C#", IsCorrect = true },
                        new CreateQuestionOptionApiRequest { Text = "Valami más", IsCorrect = false }
                    }
                }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/quizzes", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreateQuizResultDto>();
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(createRequest.Title, result.Title);
    }

    [Fact]
    public async Task GetOwnQuizzes_ShouldReturnUserQuizzes()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("getown");
        
        // Csinálunk gyorsan két kvízt ehhez a userhez
        var quiz1 = new CreateQuizApiRequest
        {
            Title = "Első Kvíz",
            Questions = new List<CreateQuizQuestionApiRequest> 
            { 
                new() { Text = "Q1", Options = [ new() { Text = "1", IsCorrect = true }, new() { Text = "2" } ] } 
            }
        };
        var quiz2 = new CreateQuizApiRequest
        {
            Title = "Második Kvíz",
            Questions = new List<CreateQuizQuestionApiRequest> 
            { 
                new() { Text = "Q1", Options = [ new() { Text = "1", IsCorrect = true }, new() { Text = "2" } ] } 
            }
        };

        await client.PostAsJsonAsync("/api/admin/quizzes", quiz1);
        await client.PostAsJsonAsync("/api/admin/quizzes", quiz2);

        // Act
        var response = await client.GetAsync("/api/admin/quizzes");

        // Assert
        response.EnsureSuccessStatusCode();
        var resultList = await response.Content.ReadFromJsonAsync<List<QuizListItemDto>>();

        Assert.NotNull(resultList);
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, q => q.Title == "Első Kvíz");
        Assert.Contains(resultList, q => q.Title == "Második Kvíz");
    }

    [Fact]
    public async Task CreateQuiz_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("createquiz_invalid");

        // 1. Üres cím
        var requestEmptyTitle = new CreateQuizApiRequest
        {
            Title = "",
            Questions = new List<CreateQuizQuestionApiRequest>
            {
                new()
                {
                    Text = "Kérdés",
                    TimeLimitSeconds = 20,
                    Options = new List<CreateQuestionOptionApiRequest>
                    {
                        new() { Text = "A", IsCorrect = true },
                        new() { Text = "B", IsCorrect = false }
                    }
                }
            }
        };

        // 2. Kérdések listája üres
        var requestNoQuestions = new CreateQuizApiRequest
        {
            Title = "Érvényes Cím",
            Questions = new List<CreateQuizQuestionApiRequest>()
        };

        // 3. Negatív időkorlát
        var requestNegativeTime = new CreateQuizApiRequest
        {
            Title = "Érvényes Cím",
            Questions = new List<CreateQuizQuestionApiRequest>
            {
                new()
                {
                    Text = "Kérdés",
                    TimeLimitSeconds = -5, // Invalid, min is 5
                    Options = new List<CreateQuestionOptionApiRequest>
                    {
                        new() { Text = "A", IsCorrect = true },
                        new() { Text = "B", IsCorrect = false }
                    }
                }
            }
        };

        // 4. Egyetlen opció sem helyes
        var requestNoCorrectOption = new CreateQuizApiRequest
        {
            Title = "Érvényes Cím",
            Questions = new List<CreateQuizQuestionApiRequest>
            {
                new()
                {
                    Text = "Kérdés",
                    TimeLimitSeconds = 20,
                    Options = new List<CreateQuestionOptionApiRequest>
                    {
                        new() { Text = "A", IsCorrect = false },
                        new() { Text = "B", IsCorrect = false }
                    }
                }
            }
        };

        // Act & Assert
        var responseEmptyTitle = await client.PostAsJsonAsync("/api/admin/quizzes", requestEmptyTitle);
        Assert.Equal(HttpStatusCode.BadRequest, responseEmptyTitle.StatusCode);

        var responseNoQuestions = await client.PostAsJsonAsync("/api/admin/quizzes", requestNoQuestions);
        Assert.Equal(HttpStatusCode.BadRequest, responseNoQuestions.StatusCode);

        var responseNegativeTime = await client.PostAsJsonAsync("/api/admin/quizzes", requestNegativeTime);
        Assert.Equal(HttpStatusCode.BadRequest, responseNegativeTime.StatusCode);

        var responseNoCorrectOption = await client.PostAsJsonAsync("/api/admin/quizzes", requestNoCorrectOption);
        Assert.Equal(HttpStatusCode.BadRequest, responseNoCorrectOption.StatusCode);
    }

    [Fact]
    public async Task GetQuiz_NotFound_ShouldReturn404()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync("getquiz_notfound");
        int nonExistentQuizId = 9999999;

        // Act
        var response = await client.GetAsync($"/api/admin/quizzes/{nonExistentQuizId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetQuiz_NotOwnedByUser_ShouldReturnForbidden()
    {
        // Arrange
        var ownerClient = await GetAuthenticatedClientAsync("quiz_owner1");
        var otherClient = await GetAuthenticatedClientAsync("quiz_owner2");

        // 1. Owner creates a quiz
        var createRequest = new CreateQuizApiRequest
        {
            Title = "Titkos Kvíz",
            Questions = new List<CreateQuizQuestionApiRequest>
            {
                new()
                {
                    Text = "Kérdés 1",
                    TimeLimitSeconds = 20,
                    Options = new List<CreateQuestionOptionApiRequest>
                    {
                        new() { Text = "A", IsCorrect = true },
                        new() { Text = "B", IsCorrect = false }
                    }
                }
            }
        };

        var createResponse = await ownerClient.PostAsJsonAsync("/api/admin/quizzes", createRequest);
        var createdQuiz = await createResponse.Content.ReadFromJsonAsync<CreateQuizResultDto>();

        // Act - Other user tries to get the quiz by ID
        var response = await otherClient.GetAsync($"/api/admin/quizzes/{createdQuiz!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
