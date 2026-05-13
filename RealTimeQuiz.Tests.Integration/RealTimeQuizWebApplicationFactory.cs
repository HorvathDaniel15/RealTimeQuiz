using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RealTimeQuiz.Data.Repositories;

namespace RealTimeQuiz.Tests.Integration;

public class RealTimeQuizWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Eltávolítjuk az eredeti (Npgsql) adatbázis beállítást
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            // Helyette In-Memory adatbázist használunk a tesztekhez
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDb");
            });
        });
    }
}

