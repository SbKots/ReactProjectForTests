using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TestManagement.Api.Application.DTOs;
using TestManagement.Api.Application.Services;
using TestManagement.Api.Application.Validation;
using TestManagement.Api.Domain.Enums;
using TestManagement.Api.Infrastructure.Data;
using TestManagement.Api.Infrastructure.Repositories;

namespace TestManagement.Tests;

[TestClass]
public sealed class TestServiceUpdateTests
{
    [TestMethod]
    public async Task UpdateAsync_WhenReplacingQuestions_KeepsPersistedQuestions()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new TestRepository(dbContext);
        var service = new TestService(repository, new TestRequestValidator());

        var created = await service.CreateAsync(new CreateTestRequest
        {
            Title = "Initial test",
            Questions =
            [
                CreateQuestion("First question"),
                CreateQuestion("Second question")
            ]
        });

        var updated = await service.UpdateAsync(created.Id, new UpdateTestRequest
        {
            Title = "Updated test",
            Questions =
            [
                CreateQuestion("Updated first question"),
                CreateQuestion("Updated second question")
            ]
        });

        var persisted = await service.GetByIdAsync(created.Id);

        Assert.AreEqual("Updated test", updated.Title);
        Assert.AreEqual(2, updated.Questions.Count);
        Assert.AreEqual(2, persisted.Questions.Count);
        Assert.AreEqual("Updated first question", persisted.Questions[0].Text);
    }

    private static QuestionWriteDto CreateQuestion(string text)
    {
        return new QuestionWriteDto
        {
            Text = text,
            Type = QuestionType.SingleChoice,
            Options =
            [
                new AnswerOptionWriteDto { Text = "Correct", IsCorrect = true },
                new AnswerOptionWriteDto { Text = "Wrong", IsCorrect = false }
            ]
        };
    }
}
