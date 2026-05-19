using TestManagement.Api.Application.DTOs;
using TestManagement.Api.Domain.Entities;

namespace TestManagement.Api.Application.Mapping;

public static class TestMappingExtensions
{
    public static TestSummaryDto ToSummaryDto(this TestDefinition test)
    {
        return new TestSummaryDto(
            test.Id,
            test.Title,
            test.Description,
            test.Questions.Count,
            test.UpdatedAtUtc);
    }

    public static TestDetailsDto ToDetailsDto(this TestDefinition test)
    {
        return new TestDetailsDto(
            test.Id,
            test.Title,
            test.Description,
            test.Questions
                .OrderBy(question => question.SortOrder)
                .Select(question => question.ToDto())
                .ToList(),
            test.CreatedAtUtc,
            test.UpdatedAtUtc);
    }

    public static TestForTakingDto ToTakingDto(this TestDefinition test)
    {
        return new TestForTakingDto(
            test.Id,
            test.Title,
            test.Description,
            test.Questions
                .OrderBy(question => question.SortOrder)
                .Select(question => new QuestionForTakingDto(
                    question.Id,
                    question.Text,
                    question.Type,
                    question.Options
                        .OrderBy(option => option.SortOrder)
                        .Select(option => new AnswerOptionForTakingDto(option.Id, option.Text))
                        .ToList()))
                .ToList());
    }

    public static TestDefinition ToEntity(this TestWriteRequest request)
    {
        var now = DateTime.UtcNow;

        var test = new TestDefinition
        {
            Title = request.Title.Trim(),
            Description = NormalizeDescription(request.Description),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        FillQuestions(test, request);

        return test;
    }

    public static void ApplyUpdate(this TestDefinition test, TestWriteRequest request)
    {
        test.ApplyMetadata(request);
        test.Questions.Clear();

        FillQuestions(test, request);
    }

    public static void ApplyMetadata(this TestDefinition test, TestWriteRequest request)
    {
        test.Title = request.Title.Trim();
        test.Description = NormalizeDescription(request.Description);
        test.UpdatedAtUtc = DateTime.UtcNow;
    }

    public static IReadOnlyList<Question> ToQuestionEntities(this TestWriteRequest request, Guid testId)
    {
        var questions = new List<Question>();
        var questionOrder = 0;

        foreach (var questionDto in request.Questions)
        {
            var question = new Question
            {
                TestDefinitionId = testId,
                Text = questionDto.Text.Trim(),
                Type = questionDto.Type,
                SortOrder = questionOrder++
            };

            var optionOrder = 0;
            foreach (var optionDto in questionDto.Options)
            {
                question.Options.Add(new AnswerOption
                {
                    Text = optionDto.Text.Trim(),
                    IsCorrect = optionDto.IsCorrect,
                    SortOrder = optionOrder++
                });
            }

            questions.Add(question);
        }

        return questions;
    }

    private static QuestionDto ToDto(this Question question)
    {
        return new QuestionDto(
            question.Id,
            question.Text,
            question.Type,
            question.Options
                .OrderBy(option => option.SortOrder)
                .Select(option => new AnswerOptionDto(option.Id, option.Text, option.IsCorrect))
                .ToList());
    }

    private static void FillQuestions(TestDefinition test, TestWriteRequest request)
    {
        var questionOrder = 0;
        foreach (var questionDto in request.Questions)
        {
            var question = new Question
            {
                TestDefinition = test,
                Text = questionDto.Text.Trim(),
                Type = questionDto.Type,
                SortOrder = questionOrder++
            };

            var optionOrder = 0;
            foreach (var optionDto in questionDto.Options)
            {
                question.Options.Add(new AnswerOption
                {
                    Question = question,
                    Text = optionDto.Text.Trim(),
                    IsCorrect = optionDto.IsCorrect,
                    SortOrder = optionOrder++
                });
            }

            test.Questions.Add(question);
        }
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
