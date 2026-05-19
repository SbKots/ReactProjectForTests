using Microsoft.EntityFrameworkCore;
using TestManagement.Api.Domain.Entities;

namespace TestManagement.Api.Infrastructure.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<TestDefinition> Tests => Set<TestDefinition>();

    public DbSet<Question> Questions => Set<Question>();

    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestDefinition>(entity =>
        {
            entity.ToTable("Tests");
            entity.HasKey(test => test.Id);
            entity.Property(test => test.Title).IsRequired().HasMaxLength(200);
            entity.Property(test => test.Description).HasMaxLength(4000);
            entity.Property(test => test.CreatedAtUtc).IsRequired();
            entity.Property(test => test.UpdatedAtUtc).IsRequired();

            entity
                .HasMany(test => test.Questions)
                .WithOne(question => question.TestDefinition)
                .HasForeignKey(question => question.TestDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions");
            entity.HasKey(question => question.Id);
            entity.Property(question => question.Text).IsRequired().HasMaxLength(1000);
            entity.Property(question => question.Type).HasConversion<string>().IsRequired().HasMaxLength(32);
            entity.Property(question => question.SortOrder).IsRequired();
            entity.HasIndex(question => new { question.TestDefinitionId, question.SortOrder });

            entity
                .HasMany(question => question.Options)
                .WithOne(option => option.Question)
                .HasForeignKey(option => option.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AnswerOption>(entity =>
        {
            entity.ToTable("AnswerOptions");
            entity.HasKey(option => option.Id);
            entity.Property(option => option.Text).IsRequired().HasMaxLength(1000);
            entity.Property(option => option.IsCorrect).IsRequired();
            entity.Property(option => option.SortOrder).IsRequired();
            entity.HasIndex(option => new { option.QuestionId, option.SortOrder });
        });
    }
}
