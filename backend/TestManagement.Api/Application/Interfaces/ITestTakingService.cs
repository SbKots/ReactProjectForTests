using TestManagement.Api.Application.DTOs;

namespace TestManagement.Api.Application.Interfaces;

public interface ITestTakingService
{
    Task<TestSubmissionResultDto> SubmitAsync(
        Guid testId,
        SubmitTestRequest request,
        CancellationToken cancellationToken = default);
}
