using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealTimeQuiz.Data.Interfaces;
using RealTimeQuiz.Data.Repositories;
using RealTimeQuiz.Logic.Services.Implementations;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.Model.Entities;
using RealTimeQuiz.WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Repositories
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizSessionRepository, QuizSessionRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<ISessionAnswerRepository, SessionAnswerRepository>();

//Logic services
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IParticipantSessionService, ParticipantSessionService>();

//Controllers + model validation response (uniform ProblemDetails for 400)
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(context.ModelState)
            {
                Title = "Request validation failed.",
                Status = StatusCodes.Status400BadRequest,
                Type = "https://httpstatuses.com/400",
                Instance = context.HttpContext.Request.Path
            };
            
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            return new BadRequestObjectResult(problem);
        };
    });

builder.Services.AddProblemDetails();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure DB schema is up-to-date and seed a stable demo owner for MVP flows.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    const string demoOwnerId = "demo-admin-1";
    var hasDemoOwner = await dbContext.Users.AnyAsync(x => x.Id == demoOwnerId);

    if (!hasDemoOwner)
    {
        dbContext.Users.Add(new ApplicationUser
        {
            Id = demoOwnerId,
            UserName = "demo.admin",
            NormalizedUserName = "DEMO.ADMIN",
            Email = "demo-admin@local.test",
            NormalizedEmail = "DEMO-ADMIN@LOCAL.TEST",
            EmailConfirmed = true
        });

        await dbContext.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Global exception handling before endpoints
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapControllers();

await app.RunAsync();