using TestManagement.Api.Application.DTOs;
using TestManagement.Api.Application.Exceptions;
using TestManagement.Api.Application.Interfaces;

namespace TestManagement.Api.Application.Services;

public sealed class TestTakingService(
    ITestRepository testRepository,
    IScoreCalculator scoreCalculator) : ITestTakingService
{
    public async Task<TestSubmissionResultDto> SubmitAsync(
        Guid testId,
        SubmitTestRequest request,
        CancellationToken cancellationToken = default)
    {
        var test = await testRepository.GetDetailsAsync(testId, trackChanges: false, cancellationToken)
            ?? throw new NotFoundException($"Test with id '{testId}' was not found.");

        ValidateSubmissionBelongsToTest(test, request);

        return scoreCalculator.Calculate(test, request);
    }

    private static void ValidateSubmissionBelongsToTest(
        Domain.Entities.TestDefinition test,
        SubmitTestRequest request)
    {
        var errors = new List<string>();
        var questionsById = test.Questions.ToDictionary(question => question.Id);

        foreach (var answer in request.Answers)
        {
            if (!questionsById.TryGetValue(answer.QuestionId, out var question))
            {
                errors.Add($"Question '{answer.QuestionId}' does not belong to this test.");
                continue;
            }

            var optionIds = question.Options.Select(option => option.Id).ToHashSet();
            var unknownOptionIds = answer.SelectedOptionIds
                .Where(optionId => !optionIds.Contains(optionId))
                .Distinct()
                .ToList();

            if (unknownOptionIds.Count > 0)
            {
                errors.Add($"Question '{answer.QuestionId}' contains answer options that do not belong to it.");
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}
