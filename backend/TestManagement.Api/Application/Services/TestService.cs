using TestManagement.Api.Application.DTOs;
using TestManagement.Api.Application.Exceptions;
using TestManagement.Api.Application.Interfaces;
using TestManagement.Api.Application.Mapping;

namespace TestManagement.Api.Application.Services;

public sealed class TestService(
    ITestRepository testRepository,
    ITestRequestValidator validator) : ITestService
{
    public async Task<IReadOnlyList<TestSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tests = await testRepository.ListAsync(cancellationToken);

        return tests
            .OrderByDescending(test => test.UpdatedAtUtc)
            .Select(test => test.ToSummaryDto())
            .ToList();
    }

    public async Task<TestDetailsDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var test = await GetExistingTestAsync(id, trackChanges: false, cancellationToken);
        return test.ToDetailsDto();
    }

    public async Task<TestForTakingDto> GetForTakingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var test = await GetExistingTestAsync(id, trackChanges: false, cancellationToken);
        return test.ToTakingDto();
    }

    public async Task<TestDetailsDto> CreateAsync(CreateTestRequest request, CancellationToken cancellationToken = default)
    {
        validator.Validate(request);

        var test = request.ToEntity();

        await testRepository.AddAsync(test, cancellationToken);
        await testRepository.SaveChangesAsync(cancellationToken);

        return test.ToDetailsDto();
    }

    public async Task<TestDetailsDto> UpdateAsync(
        Guid id,
        UpdateTestRequest request,
        CancellationToken cancellationToken = default)
    {
        validator.Validate(request);

        var test = await testRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Test with id '{id}' was not found.");

        await testRepository.DeleteQuestionsAsync(id, cancellationToken);
        test.ApplyUpdate(request);

        await testRepository.SaveChangesAsync(cancellationToken);

        return test.ToDetailsDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var test = await GetExistingTestAsync(id, trackChanges: true, cancellationToken);

        testRepository.Remove(test);
        await testRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Domain.Entities.TestDefinition> GetExistingTestAsync(
        Guid id,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        return await testRepository.GetDetailsAsync(id, trackChanges, cancellationToken)
            ?? throw new NotFoundException($"Test with id '{id}' was not found.");
    }
}
