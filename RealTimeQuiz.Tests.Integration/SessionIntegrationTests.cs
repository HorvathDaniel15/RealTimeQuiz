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
    public async Task SubmitAnswer_AfterTimeLimit_ShouldReturnBadRequest()
    {
        // Arrange
        var adminClient = await GetAuthenticatedClientAsync("submit_after_time");
        
        var createQuizReq = new CreateQuizApiRequest
        {
            Title = "Gyors Kvíz",
            Questions = new List<CreateQuizQuestionApiRequest>
            {
                new()
                {
                    Text = "Gyors kérdés",
                    TimeLimitSeconds = 5,
                    Options = new List<CreateQuestionOptionApiRequest>
                    {
                        new() { Text = "A", IsCorrect = true },
                        new() { Text = "B", IsCorrect = false }
                    }
                }
            }
        };
        var quizResp = await adminClient.PostAsJsonAsync("/api/admin/quizzes", createQuizReq);
        quizResp.EnsureSuccessStatusCode();
        var quiz = await quizResp.Content.ReadFromJsonAsync<CreateQuizResultDto>();

        var createSessionResponse = await adminClient.PostAsJsonAsync("/api/admin/sessions", new CreateSessionApiRequest { QuizId = quiz!.Id });
        var session = await createSessionResponse.Content.ReadFromJsonAsync<CreateSessionResultDto>();
        await adminClient.PostAsync($"/api/admin/sessions/{session!.Id}/open-lobby", null);

        var participantClient = Factory.CreateClient();
        var joinRequest = new JoinSessionByPinApiRequest { JoinPin = session.JoinPin, DisplayName = "Slowpoke" };
        var joinResponse = await participantClient.PostAsJsonAsync("/api/participant-sessions/join", joinRequest);
        var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinSessionResultDto>();

        await adminClient.PostAsync($"/api/admin/sessions/{session.Id}/start", null);

        var currentQuestionResponse = await participantClient.GetAsync($"/api/participant-sessions/{joinResult!.ParticipantId}/current-question");
        currentQuestionResponse.EnsureSuccessStatusCode();
        var currentQuestion = await currentQuestionResponse.Content.ReadFromJsonAsync<ParticipantCurrentQuestionDto>();

        await Task.Delay(5200);

        // Act
        var submitRequest = new SubmitAnswerApiRequest
        {
            ParticipantId = joinResult.ParticipantId,
            QuestionId = currentQuestion!.QuestionId,
            OptionId = currentQuestion.Options.First().Id
        };
        var submitResponse = await participantClient.PostAsJsonAsync("/api/participant-sessions/submit-answer", submitRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, submitResponse.StatusCode);
    }

    [Fact]
    public async Task CreateSession_ShouldReturnCreatedSessionWithPin()
    {
        // Arrange
        var adminClient = await GetAuthenticatedClientAsync("createsession");
        var quiz = await CreateTestQuizAsync(adminClient);

        // Act
        var request = new CreateSessionApiRequest { QuizId = quiz.Id };
        var response = await adminClient.PostAsJsonAsync("/api/admin/sessions", request);

        // Assert
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
        // Arrange
        var adminClient = await GetAuthenticatedClientAsync("joinsession");
        var quiz = await CreateTestQuizAsync(adminClient);
        
        var createSessionResponse = await adminClient.PostAsJsonAsync("/api/admin/sessions", new CreateSessionApiRequest { QuizId = quiz.Id });
        var session = await createSessionResponse.Content.ReadFromJsonAsync<CreateSessionResultDto>();

        await adminClient.PostAsync($"/api/admin/sessions/{session!.Id}/open-lobby", null);

        var participantClient = Factory.CreateClient();
        participantClient.DefaultRequestHeaders.Add("X-User-Id", "test_guest_1");
        
        var joinRequest = new JoinSessionByPinApiRequest
        {
            JoinPin = session!.JoinPin,
            DisplayName = "Player1"
        };
        
        // Act
        var response = await participantClient.PostAsJsonAsync("/api/participant-sessions/join", joinRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JoinSessionResultDto>();
        Assert.NotNull(result);
        Assert.True(result.ParticipantId > 0);
        Assert.Equal(session.Id, result.SessionId);
    }

    [Fact]
    public async Task JoinSession_WithInvalidPin_ShouldReturnNotFoundOrBadRequest()
    {
        // Arrange
        var participantClient = Factory.CreateClient();
        participantClient.DefaultRequestHeaders.Add("X-User-Id", "test_guest_2");
        
        var joinRequest = new JoinSessionByPinApiRequest
        {
            JoinPin = "999999",
            DisplayName = "Hacker"
        };

        // Act
        var response = await participantClient.PostAsJsonAsync("/api/participant-sessions/join", joinRequest);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task JoinSession_WhenSessionAlreadyStarted_ShouldReturnBadRequest()
    {
        // Arrange
        var adminClient = await GetAuthenticatedClientAsync("joinsession_started");
        var quiz = await CreateTestQuizAsync(adminClient);
        
        var createSessionResponse = await adminClient.PostAsJsonAsync("/api/admin/sessions", new CreateSessionApiRequest { QuizId = quiz.Id });
        var session = await createSessionResponse.Content.ReadFromJsonAsync<CreateSessionResultDto>();

        await adminClient.PostAsync($"/api/admin/sessions/{session!.Id}/open-lobby", null);

        await adminClient.PostAsync($"/api/admin/sessions/{session.Id}/start", null);

        var participantClient = Factory.CreateClient();
        participantClient.DefaultRequestHeaders.Add("X-User-Id", "test_guest_late");
        
        var joinRequest = new JoinSessionByPinApiRequest
        {
            JoinPin = session.JoinPin,
            DisplayName = "LatePlayer"
        };
        
        // Act
        var response = await participantClient.PostAsJsonAsync("/api/participant-sessions/join", joinRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task JoinSession_WhenSessionInDraft_ShouldReturnForbidden()
    {
        // Arrange
        var adminClient = await GetAuthenticatedClientAsync("joinsession_draft");
        var quiz = await CreateTestQuizAsync(adminClient);
        
        var createSessionResponse = await adminClient.PostAsJsonAsync("/api/admin/sessions", new CreateSessionApiRequest { QuizId = quiz.Id });
        var session = await createSessionResponse.Content.ReadFromJsonAsync<CreateSessionResultDto>();

        var participantClient = Factory.CreateClient();
        
        var joinRequest = new JoinSessionByPinApiRequest
        {
            JoinPin = session!.JoinPin,
            DisplayName = "EarlyBird"
        };
        
        // Act
        var response = await participantClient.PostAsJsonAsync("/api/participant-sessions/join", joinRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task StartSession_ByNonOwner_ShouldReturnForbidden()
    {
        // Arrange
        var ownerClient = await GetAuthenticatedClientAsync("real_owner");
        var maliciousClient = await GetAuthenticatedClientAsync("malicious_user");

        var quiz = await CreateTestQuizAsync(ownerClient);

        var createSessionResponse = await ownerClient.PostAsJsonAsync("/api/admin/sessions", new CreateSessionApiRequest { QuizId = quiz.Id });
        var session = await createSessionResponse.Content.ReadFromJsonAsync<CreateSessionResultDto>();

        await ownerClient.PostAsync($"/api/admin/sessions/{session!.Id}/open-lobby", null);

        // Act
        var response = await maliciousClient.PostAsync($"/api/admin/sessions/{session.Id}/start", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
