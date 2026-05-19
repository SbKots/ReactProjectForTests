using TestManagement.Api.Application.DTOs;

namespace TestManagement.Api.Application.Interfaces;

public interface ITestService
{
    Task<IReadOnlyList<TestSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TestDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TestForTakingDto> GetForTakingAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TestDetailsDto> CreateAsync(CreateTestRequest request, CancellationToken cancellationToken = default);

    Task<TestDetailsDto> UpdateAsync(Guid id, UpdateTestRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
