using TestManagement.Api.Domain.Entities;

namespace TestManagement.Api.Application.Interfaces;

public interface ITestRepository : IRepository<TestDefinition>
{
    Task<IReadOnlyList<TestDefinition>> ListAsync(CancellationToken cancellationToken = default);

    Task<TestDefinition?> GetDetailsAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken = default);

    Task ReplaceQuestionsAsync(
        Guid testId,
        IReadOnlyList<Question> questions,
        CancellationToken cancellationToken = default);
}
