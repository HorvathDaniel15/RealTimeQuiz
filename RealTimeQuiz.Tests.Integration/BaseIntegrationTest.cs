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

    /// <summary>
    /// Segédfüggvény, ha a tesztek során közvetlenül bele kell nyúlni az In-Memory adatbázisba.
    /// </summary>
    protected IServiceScope CreateScope()
    {
        return Factory.Services.CreateScope();
    }

    /// <summary>
    /// Kinyeri az AppDbContext-et a létrehozott scope-ból.
    /// Használata:
    /// using var scope = CreateScope();
    /// var db = GetDbContext(scope);
    /// </summary>
    protected AppDbContext GetDbContext(IServiceScope scope)
    {
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    /// <summary>
    /// Segédfüggvény: Regisztrál egy egyedi felhasználót, bejelentkezik vele, és visszaad egy autentikált HttpClient-et.
    /// Ezzel minden teszt izolált, saját felhasználóval fog dolgozni.
    /// </summary>
    protected async Task<HttpClient> GetAuthenticatedClientAsync(string uniqueSuffix)
    {
        var email = $"test_{uniqueSuffix}@local.test";
        var password = "TestPassword123!";
        var username = $"user_{uniqueSuffix}";

        // 1. Regisztráció
        var registerRequest = new RealTimeQuiz.Logic.Contracts.Auth.Requests.RegisterRequest 
        { 
            Email = email, 
            Password = password, 
            UserName = username 
        };
        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // 2. Belépés
        var loginRequest = new RealTimeQuiz.Logic.Contracts.Auth.Requests.LoginRequest 
        { 
            Email = email, 
            Password = password 
        };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<RealTimeQuiz.Logic.Contracts.Auth.Responses.LoginResult>();

        // 3. Új HttpClient belépett header-rel
        var authClient = Factory.CreateClient();
        authClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        return authClient;
    }
}
