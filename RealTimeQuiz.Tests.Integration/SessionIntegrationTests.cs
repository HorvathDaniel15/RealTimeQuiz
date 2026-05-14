using System.Net;
using System.Net.Http.Json;
using RealTimeQuiz.Logic.Contracts.Quizzes.Responses;
using RealTimeQuiz.Logic.Contracts.Sessions.Responses;
using RealTimeQuiz.WebAPI.Contracts.Requests.Admin;
using RealTimeQuiz.WebAPI.Contracts.Requests.Participant;

namespace RealTimeQuiz.Tests.Integration;

public class SessionIntegrationTests : BaseIntegrationTest
{
    public SessionIntegrationTests(RealTimeQuizWebApplicationFactory factory) : base(factory)
    {
    }

    private async Task<CreateQuizResultDto> CreateTestQuizAsync(HttpClient adminClient)
    {
        var createRequest = new CreateQuizApiRequest
        {
            Title = "Session Teszt Kvíz",
            Questions = new List<CreateQuizQuestionApiRequest>
            {
                new()
                {
                    Text = "Melyik egy autó márka?",
                    TimeLimitSeconds = 30,
                    Options = new List<CreateQuestionOptionApiRequest>
                    {
                        new() { Text = "BMW", IsCorrect = true },
                        new() { Text = "Alma", IsCorrect = false }
                    }
                }
            }
        };

        var response = await adminClient.PostAsJsonAsync("/api/admin/quizzes", createRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateQuizResultDto>();
        return result!;
    }

    [Fact]
    public async Task CreateSession_ShouldReturnCreatedSessionWithPin()
    {
        var adminClient = await GetAuthenticatedClientAsync("createsession");
        var quiz = await CreateTestQuizAsync(adminClient);

        var request = new CreateSessionApiRequest { QuizId = quiz.Id };
        var response = await adminClient.PostAsJsonAsync("/api/admin/sessions", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateSessionResultDto>();

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(quiz.Id, result.QuizId);
        Assert.Equal(6, result.JoinPin.Length); 
        Assert.Equal(RealTimeQuiz.Model.Enums.SessionState.Draft, result.State);
    }

    [Fact]
    public async Task JoinSession_WithValidPin_ShouldReturnSuccess()
    {
        var adminClient = await GetAuthenticatedClientAsync("joinsession");
        var quiz = await CreateTestQuizAsync(adminClient);
        
        // 1. Create
        var createSessionResponse = await adminClient.PostAsJsonAsync("/api/admin/sessions", new CreateSessionApiRequest { QuizId = quiz.Id });
        var session = await createSessionResponse.Content.ReadFromJsonAsync<CreateSessionResultDto>();

        // 2. Open Lobby
        await adminClient.PostAsync($"/api/admin/sessions/{session!.Id}/open-lobby", null);

        var participantClient = Factory.CreateClient();
        participantClient.DefaultRequestHeaders.Add("X-User-Id", "test_guest_1");
        
        var joinRequest = new JoinSessionByPinApiRequest
        {
            JoinPin = session!.JoinPin,
            DisplayName = "Player1"
        };
        
        var response = await participantClient.PostAsJsonAsync("/api/participant-sessions/join", joinRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JoinSessionResultDto>();
        Assert.NotNull(result);
        Assert.True(result.ParticipantId > 0);
        Assert.Equal(session.Id, result.SessionId);
    }

    [Fact]
    public async Task JoinSession_WithInvalidPin_ShouldReturnNotFoundOrBadRequest()
    {
        var participantClient = Factory.CreateClient();
        participantClient.DefaultRequestHeaders.Add("X-User-Id", "test_guest_2");
        
        var joinRequest = new JoinSessionByPinApiRequest
        {
            JoinPin = "999999",
            DisplayName = "Hacker"
        };

        var response = await participantClient.PostAsJsonAsync("/api/participant-sessions/join", joinRequest);

        Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest);
    }
}
