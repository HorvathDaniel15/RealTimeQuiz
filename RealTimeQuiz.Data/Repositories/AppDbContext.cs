using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealTimeQuiz.Model.Entities;

namespace RealTimeQuiz.Data.Repositories;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<QuizSession> QuizSessions => Set<QuizSession>();
    public DbSet<SessionParticipant> SessionParticipants => Set<SessionParticipant>();
    public DbSet<SessionAnswer> SessionAnswers => Set<SessionAnswer>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        ConfigureQuiz(builder);
        ConfigureQuizQuestion(builder);
        ConfigureQuestionOption(builder);
        ConfigureQuizSession(builder);
        ConfigureSessionParticipant(builder);
        ConfigureSessionAnswer(builder);
    }
    
    private static void ConfigureQuiz(ModelBuilder builder)
    {
        builder.Entity<Quiz>(entity =>
        {
            entity.ToTable("Quizzes");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Description)
                .HasMaxLength(2000);

            entity.Property(x => x.OwnerId)
                .IsRequired();

            entity.HasOne(x => x.Owner)
                .WithMany(x => x.OwnedQuizzes)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Questions)
                .WithOne(x => x.Quiz)
                .HasForeignKey(x => x.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Sessions)
                .WithOne(x => x.Quiz)
                .HasForeignKey(x => x.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureQuizQuestion(ModelBuilder builder)
    {
        builder.Entity<QuizQuestion>(entity =>
        {
            entity.ToTable("QuizQuestions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Text)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(x => x.ImageUrl)
                .HasMaxLength(1000);

            entity.HasIndex(x => new { x.QuizId, x.OrderIndex })
                .IsUnique();

            entity.HasMany(x => x.Options)
                .WithOne(x => x.Question)
                .HasForeignKey(x => x.QuizQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureQuestionOption(ModelBuilder builder)
    {
        builder.Entity<QuestionOption>(entity =>
        {
            entity.ToTable("QuestionOptions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Text)
                .IsRequired()
                .HasMaxLength(300);

            entity.HasIndex(x => new { x.QuizQuestionId, x.OrderIndex })
                .IsUnique();
        });
    }

    private static void ConfigureQuizSession(ModelBuilder builder)
    {
        builder.Entity<QuizSession>(entity =>
        {
            entity.ToTable("QuizSessions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.JoinPin)
                .IsRequired()
                .HasMaxLength(10);

            entity.HasIndex(x => x.JoinPin)
                .IsUnique();

            entity.Property(x => x.State)
                .HasConversion<int>();

            entity.HasMany(x => x.Participants)
                .WithOne(x => x.QuizSession)
                .HasForeignKey(x => x.QuizSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Answers)
                .WithOne(x => x.QuizSession)
                .HasForeignKey(x => x.QuizSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureSessionParticipant(ModelBuilder builder)
    {
        builder.Entity<SessionParticipant>(entity =>
        {
            entity.ToTable("SessionParticipants");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Participations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => new { x.QuizSessionId, x.DisplayName })
                .IsUnique();
        });
    }

    private static void ConfigureSessionAnswer(ModelBuilder builder)
    {
        builder.Entity<SessionAnswer>(entity =>
        {
            entity.ToTable("SessionAnswers");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Status)
                .HasConversion<int>();

            entity.HasOne(x => x.SessionParticipant)
                .WithMany(x => x.Answers)
                .HasForeignKey(x => x.SessionParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.QuizQuestion)
                .WithMany(x => x.Answers)
                .HasForeignKey(x => x.QuizQuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.QuestionOption)
                .WithMany()
                .HasForeignKey(x => x.QuestionOptionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.SessionParticipantId, x.QuizQuestionId })
                .IsUnique();
        });
    }
}