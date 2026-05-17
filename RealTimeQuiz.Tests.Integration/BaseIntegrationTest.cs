using Microsoft.Extensions.DependencyInjection;
using RealTimeQuiz.Data.Repositories;
using System.Net.Http.Json;

namespace RealTimeQuiz.Tests.Integration;

public abstract class BaseIntegrationTest : IClassFixture<RealTimeQuizWebApplicationFactory>
{
    protected readonly RealTimeQuizWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(RealTimeQuizWebApplicationFactory factory)
    {
        Factory = factory;
        Client = Factory.CreateClient();
    }
    
    protected IServiceScope CreateScope()
    {
        return Factory.Services.CreateScope();
    }
    
    protected AppDbContext GetDbContext(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
    
    protected async Task<HttpClient> GetAuthenticatedClientAsync(string uniqueSuffix)
    {
        var email = $"test_{uniqueSuffix}@local.test";
        var password = "TestPassword123!";
        var username = $"user_{uniqueSuffix}";

        // 1. Registration
        var registerRequest = new RealTimeQuiz.Logic.Contracts.Auth.Requests.RegisterRequest 
        { 
            Email = email, 
            Password = password, 
            UserName = username 
        };
        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // 2. Login
        var loginRequest = new RealTimeQuiz.Logic.Contracts.Auth.Requests.LoginRequest 
        { 
            Email = email, 
            Password = password 
        };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<RealTimeQuiz.Logic.Contracts.Auth.Responses.LoginResult>();

        // 3. New HttpClient with header
        var authClient = Factory.CreateClient();
        authClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        return authClient;
    }
}
