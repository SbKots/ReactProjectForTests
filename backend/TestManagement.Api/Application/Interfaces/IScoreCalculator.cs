using TestManagement.Api.Application.DTOs;
using TestManagement.Api.Domain.Entities;

namespace TestManagement.Api.Application.Interfaces;

public interface IScoreCalculator
{
    TestSubmissionResultDto Calculate(TestDefinition test, SubmitTestRequest request);
}
