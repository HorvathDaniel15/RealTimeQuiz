using JWTOauth2.AspNetCore.Extensions;
using JWTOauth2.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RealTimeQuiz.Data.Interfaces;
using RealTimeQuiz.Data.Repositories;
using RealTimeQuiz.Logic.Services.Implementations;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.Model.Entities;
using RealTimeQuiz.WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

const string DevCorsPolicy = "DevCorsPolicy";

// Database
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Identity (UserManager/RoleManager/SignInManager registrations)
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

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
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ===== JWT/OAuth2 infra =====
// 1) AuthN + AuthZ
builder.Services.AddJWTOauth2(options =>
{
    var auth = builder.Configuration.GetSection("Auth");
    options.Issuer = auth["Issuer"]!;
    options.Audience = auth["Audience"]!;
    options.AccessTokenSecret = auth["AccessTokenSecret"]!;
    options.RefreshTokenSecret = auth["RefreshTokenSecret"]!;
    options.AccessTokenExpiration = TimeSpan.FromMinutes(int.Parse(auth["AccessTokenMinutes"]!));
    options.RefreshTokenExpiration = TimeSpan.FromDays(int.Parse(auth["RefreshTokenDays"]!));
});

builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
builder.Services.AddScoped<IAuthService, AuthService>();


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RealTimeQuiz API",
        Version = "v1",
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.",
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

app.UseCors(DevCorsPolicy);

// Global exception handling before endpoints
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();