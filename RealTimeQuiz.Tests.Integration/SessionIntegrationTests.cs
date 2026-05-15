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
        var adminClient = await GetAuthenticatedClientAsync("submit_after_time");
        
        // 1. Create a quiz with the minimum allowed time limit (5 seconds)
        var createQuizReq = new CreateQuizApiRequest
        {
            Title = "Gyors Kvíz",
            Questions = new List<CreateQuizQuestionApiRequest>
            {
                new()
                {
                    Text = "Gyors kérdés",
                    TimeLimitSeconds = 5, // A minimum időkorlát 5 másodperc
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

        // 2. Create session & Open Lobby
        var createSessionResponse = await adminClient.PostAsJsonAsync("/api/admin/sessions", new CreateSessionApiRequest { QuizId = quiz!.Id });
        var session = await createSessionResponse.Content.ReadFromJsonAsync<CreateSessionResultDto>();
        await adminClient.PostAsync($"/api/admin/sessions/{session!.Id}/open-lobby", null);

        // 3. Join as Participant
        var participantClient = Factory.CreateClient();
        var joinRequest = new JoinSessionByPinApiRequest { JoinPin = session.JoinPin, DisplayName = "Slowpoke" };
        var joinResponse = await participantClient.PostAsJsonAsync("/api/participant-sessions/join", joinRequest);
        var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinSessionResultDto>();

        // 4. Start Session (starts the 1-second timer)
        await adminClient.PostAsync($"/api/admin/sessions/{session.Id}/start", null);

        // Fetch current question for the participant to get valid IDs
        var currentQuestionResponse = await participantClient.GetAsync($"/api/participant-sessions/{joinResult!.ParticipantId}/current-question");
        currentQuestionResponse.EnsureSuccessStatusCode();
        var currentQuestion = await currentQuestionResponse.Content.ReadFromJsonAsync<ParticipantCurrentQuestionDto>();

        // 5. Wait for the time limit to expire (le kell várni az 5 másodpercet + pici rátartás)
        await Task.Delay(5200);

        // 6. Try to submit an answer
        var submitRequest = new SubmitAnswerApiRequest
        {
            ParticipantId = joinResult.ParticipantId,
            QuestionId = currentQuestion!.QuestionId,
            OptionId = currentQuestion.Options.First().Id
        };
        var submitResponse = await participantClient.PostAsJsonAsync("/api/participant-sessions/submit-answer", submitRequest);

        // 7. Assert it is rejected
        Assert.Equal(HttpStatusCode.BadRequest, submitResponse.StatusCode);
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

    [Fact]
    public async Task JoinSession_WhenSessionAlreadyStarted_ShouldReturnBadRequest()
    {
        var adminClient = await GetAuthenticatedClientAsync("joinsession_started");
        var quiz = await CreateTestQuizAsync(adminClient);
        
        // 1. Create
        var createSessionResponse = await adminClient.PostAsJsonAsync("/api/admin/sessions", new CreateSessionApiRequest { QuizId = quiz.Id });
        var session = await createSessionResponse.Content.ReadFromJsonAsync<CreateSessionResultDto>();

        // 2. Open Lobby
        await adminClient.PostAsync($"/api/admin/sessions/{session!.Id}/open-lobby", null);

        // 3. Start Session
        await adminClient.PostAsync($"/api/admin/sessions/{session.Id}/start", null);

        // 4. Try to Join
        var participantClient = Factory.CreateClient();
        participantClient.DefaultRequestHeaders.Add("X-User-Id", "test_guest_late");
        
        var joinRequest = new JoinSessionByPinApiRequest
        {
            JoinPin = session.JoinPin,
            DisplayName = "LatePlayer"
        };
        
        var response = await participantClient.PostAsJsonAsync("/api/participant-sessions/join", joinRequest);

        // Késői csatlakozást vissza kellene utasítani (A jelenlegi logika alapján ForbiddenOperationException keletkezik, ami Forbidden kódot ad)
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task JoinSession_WhenSessionInDraft_ShouldReturnForbidden()
    {
        var adminClient = await GetAuthenticatedClientAsync("joinsession_draft");
        var quiz = await CreateTestQuizAsync(adminClient);
        
        // 1. Create (State will be Draft)
        var createSessionResponse = await adminClient.PostAsJsonAsync("/api/admin/sessions", new CreateSessionApiRequest { QuizId = quiz.Id });
        var session = await createSessionResponse.Content.ReadFromJsonAsync<CreateSessionResultDto>();

        // 2. Try to Join without Open Lobby
        var participantClient = Factory.CreateClient();
        
        var joinRequest = new JoinSessionByPinApiRequest
        {
            JoinPin = session!.JoinPin,
            DisplayName = "EarlyBird"
        };
        
        var response = await participantClient.PostAsJsonAsync("/api/participant-sessions/join", joinRequest);

        // A jelenlegi logika alapján ez is Forbidden kell legyen, mivel a state nem Lobby
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task StartSession_ByNonOwner_ShouldReturnForbidden()
    {
        // Arrange
        var ownerClient = await GetAuthenticatedClientAsync("real_owner");
        var maliciousClient = await GetAuthenticatedClientAsync("malicious_user");

        var quiz = await CreateTestQuizAsync(ownerClient);

        // Owner creates session
        var createSessionResponse = await ownerClient.PostAsJsonAsync("/api/admin/sessions", new CreateSessionApiRequest { QuizId = quiz.Id });
        var session = await createSessionResponse.Content.ReadFromJsonAsync<CreateSessionResultDto>();

        // Owner opens lobby
        await ownerClient.PostAsync($"/api/admin/sessions/{session!.Id}/open-lobby", null);

        // Act - Malicious user tries to start it
        var response = await maliciousClient.PostAsync($"/api/admin/sessions/{session.Id}/start", null);

        // Assert - Különböző felhasználók ne indíthassák el mások kvízét
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
