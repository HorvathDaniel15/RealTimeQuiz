using Microsoft.EntityFrameworkCore;
using RealTimeQuiz.Data.Interfaces;
using RealTimeQuiz.Data.Repositories;
using RealTimeQuiz.Logic.Services.Implementations;
using RealTimeQuiz.Logic.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizSessionRepository, QuizSessionRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<ISessionAnswerRepository, SessionAnswerRepository>();

builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IParticipantSessionService, ParticipantSessionService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

await app.RunAsync();