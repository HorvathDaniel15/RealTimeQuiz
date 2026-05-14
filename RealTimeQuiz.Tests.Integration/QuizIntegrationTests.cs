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
}
