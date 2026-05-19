using Microsoft.EntityFrameworkCore;
using TestManagement.Api.Application.Interfaces;
using TestManagement.Api.Domain.Entities;
using TestManagement.Api.Infrastructure.Data;

namespace TestManagement.Api.Infrastructure.Repositories;

public sealed class TestRepository : EfRepository<TestDefinition>, ITestRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TestRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TestDefinition>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await BaseQuery(trackChanges: false)
            .ToListAsync(cancellationToken);
    }

    public async Task<TestDefinition?> GetDetailsAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken = default)
    {
        return await BaseQuery(trackChanges)
            .FirstOrDefaultAsync(test => test.Id == id, cancellationToken);
    }

    public async Task ReplaceQuestionsAsync(
        Guid testId,
        IReadOnlyList<Question> questions,
        CancellationToken cancellationToken = default)
    {
        var questionIds = _dbContext.Questions
            .Where(question => question.TestDefinitionId == testId)
            .Select(question => question.Id);

        await _dbContext.AnswerOptions
            .Where(option => questionIds.Contains(option.QuestionId))
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Questions
            .Where(question => question.TestDefinitionId == testId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Questions.AddRangeAsync(questions, cancellationToken);
    }

    private IQueryable<TestDefinition> BaseQuery(bool trackChanges)
    {
        var query = _dbContext.Tests
            .Include(test => test.Questions.OrderBy(question => question.SortOrder))
            .ThenInclude(question => question.Options.OrderBy(option => option.SortOrder))
            .AsSplitQuery()
            .AsQueryable();

        return trackChanges ? query : query.AsNoTracking();
    }
}
