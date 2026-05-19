using TestManagement.Api.Application.DTOs;
using TestManagement.Api.Application.Interfaces;
using TestManagement.Api.Domain.Entities;
using TestManagement.Api.Domain.Enums;

namespace TestManagement.Api.Application.Services;

public sealed class ScoreCalculator : IScoreCalculator
{
    public TestSubmissionResultDto Calculate(TestDefinition test, SubmitTestRequest request)
    {
        var submittedAnswers = request.Answers
            .GroupBy(answer => answer.QuestionId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .SelectMany(answer => answer.SelectedOptionIds)
                    .Distinct()
                    .ToHashSet());

        var questionScores = test.Questions
            .OrderBy(question => question.SortOrder)
            .Select(question => CalculateQuestionResult(question, submittedAnswers))
            .ToList();

        var rawScore = questionScores.Sum(result => result.RawScore);
        var score = Math.Round(rawScore, 2, MidpointRounding.AwayFromZero);
        var maxScore = test.Questions.Count;
        var percentage = maxScore == 0
            ? 0
            : Math.Round(rawScore / maxScore * 100, 1, MidpointRounding.AwayFromZero);
        var questionResults = questionScores.Select(result => result.Dto).ToList();

        return new TestSubmissionResultDto(test.Id, score, maxScore, percentage, questionResults);
    }

    private static QuestionScoreCalculation CalculateQuestionResult(
        Question question,
        IReadOnlyDictionary<Guid, HashSet<Guid>> submittedAnswers)
    {
        var selectedOptionIds = submittedAnswers.TryGetValue(question.Id, out var selected)
            ? selected
            : [];

        var correctOptionIds = question.Options
            .Where(option => option.IsCorrect)
            .Select(option => option.Id)
            .ToHashSet();

        var score = question.Type switch
        {
            QuestionType.SingleChoice => CalculateSingleChoiceScore(selectedOptionIds, correctOptionIds),
            QuestionType.MultipleChoice => CalculateMultipleChoiceScore(selectedOptionIds, correctOptionIds),
            _ => 0
        };

        return new QuestionScoreCalculation(
            score,
            new QuestionResultDto(
                question.Id,
                Math.Round(score, 2, MidpointRounding.AwayFromZero),
                1,
                correctOptionIds.ToList(),
                selectedOptionIds.ToList()));
    }

    private static decimal CalculateSingleChoiceScore(
        IReadOnlySet<Guid> selectedOptionIds,
        IReadOnlySet<Guid> correctOptionIds)
    {
        return selectedOptionIds.Count == 1 && correctOptionIds.Contains(selectedOptionIds.Single())
            ? 1
            : 0;
    }

    private static decimal CalculateMultipleChoiceScore(
        IReadOnlySet<Guid> selectedOptionIds,
        IReadOnlySet<Guid> correctOptionIds)
    {
        if (correctOptionIds.Count == 0)
        {
            return 0;
        }

        var answerWeight = 1m / correctOptionIds.Count;
        var correctlyMarked = selectedOptionIds.Count(correctOptionIds.Contains);
        var incorrectlyMarked = selectedOptionIds.Count(optionId => !correctOptionIds.Contains(optionId));
        var rawScore = (correctlyMarked - incorrectlyMarked) * answerWeight;

        return Math.Max(0, rawScore);
    }

    private sealed record QuestionScoreCalculation(decimal RawScore, QuestionResultDto Dto);
}
