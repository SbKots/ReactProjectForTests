using TestManagement.Api.Application.DTOs;
using TestManagement.Api.Application.Services;
using TestManagement.Api.Domain.Entities;
using TestManagement.Api.Domain.Enums;

namespace TestManagement.Tests;

[TestClass]
public sealed class ScoreCalculatorTests
{
    [TestMethod]
    public void Calculate_WhenSingleChoiceAnswerIsCorrect_ReturnsOnePoint()
    {
        var correctOptionId = Guid.NewGuid();
        var wrongOptionId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var test = CreateTest(
            questionId,
            QuestionType.SingleChoice,
            correctOptionIds: [correctOptionId],
            wrongOptionIds: [wrongOptionId]);
        var request = Submit(questionId, correctOptionId);

        var result = new ScoreCalculator().Calculate(test, request);

        Assert.AreEqual(1, result.Score);
        Assert.AreEqual(100, result.Percentage);
    }

    [TestMethod]
    public void Calculate_WhenMultipleChoiceHasWrongSelection_SubtractsAnswerWeight()
    {
        var firstCorrectOptionId = Guid.NewGuid();
        var secondCorrectOptionId = Guid.NewGuid();
        var wrongOptionId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var test = CreateTest(
            questionId,
            QuestionType.MultipleChoice,
            correctOptionIds: [firstCorrectOptionId, secondCorrectOptionId],
            wrongOptionIds: [wrongOptionId]);
        var request = Submit(questionId, firstCorrectOptionId, wrongOptionId);

        var result = new ScoreCalculator().Calculate(test, request);

        Assert.AreEqual(0, result.Score);
        Assert.AreEqual(0, result.Percentage);
    }

    [TestMethod]
    public void Calculate_WhenMultipleChoiceIsPartiallyCorrect_ReturnsFractionalScore()
    {
        var firstCorrectOptionId = Guid.NewGuid();
        var secondCorrectOptionId = Guid.NewGuid();
        var thirdCorrectOptionId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var test = CreateTest(
            questionId,
            QuestionType.MultipleChoice,
            correctOptionIds: [firstCorrectOptionId, secondCorrectOptionId, thirdCorrectOptionId],
            wrongOptionIds: []);
        var request = Submit(questionId, firstCorrectOptionId, secondCorrectOptionId);

        var result = new ScoreCalculator().Calculate(test, request);

        Assert.AreEqual(0.67m, result.Score);
        Assert.AreEqual(66.7m, result.Percentage);
    }

    private static TestDefinition CreateTest(
        Guid questionId,
        QuestionType questionType,
        IReadOnlyList<Guid> correctOptionIds,
        IReadOnlyList<Guid> wrongOptionIds)
    {
        var test = new TestDefinition
        {
            Id = Guid.NewGuid(),
            Title = "Scoring test"
        };

        var question = new Question
        {
            Id = questionId,
            TestDefinition = test,
            Text = "Question",
            Type = questionType,
            SortOrder = 0
        };

        var sortOrder = 0;
        foreach (var optionId in correctOptionIds)
        {
            question.Options.Add(new AnswerOption
            {
                Id = optionId,
                Question = question,
                Text = $"Correct {sortOrder + 1}",
                IsCorrect = true,
                SortOrder = sortOrder++
            });
        }

        foreach (var optionId in wrongOptionIds)
        {
            question.Options.Add(new AnswerOption
            {
                Id = optionId,
                Question = question,
                Text = $"Wrong {sortOrder + 1}",
                IsCorrect = false,
                SortOrder = sortOrder++
            });
        }

        test.Questions.Add(question);
        return test;
    }

    private static SubmitTestRequest Submit(Guid questionId, params Guid[] selectedOptionIds)
    {
        return new SubmitTestRequest
        {
            Answers =
            [
                new QuestionAnswerSubmissionDto
                {
                    QuestionId = questionId,
                    SelectedOptionIds = selectedOptionIds
                }
            ]
        };
    }
}
