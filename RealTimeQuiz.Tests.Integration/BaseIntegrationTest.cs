using Microsoft.Extensions.DependencyInjection;
using RealTimeQuiz.Data.Repositories;

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
}

