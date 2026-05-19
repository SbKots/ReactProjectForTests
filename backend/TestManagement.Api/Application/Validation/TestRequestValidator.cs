using TestManagement.Api.Application.DTOs;
using TestManagement.Api.Application.Exceptions;
using TestManagement.Api.Application.Interfaces;
using TestManagement.Api.Domain.Enums;

namespace TestManagement.Api.Application.Validation;

public sealed class TestRequestValidator : ITestRequestValidator
{
    public void Validate(TestWriteRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add("Test title is required.");
        }
        else if (request.Title.Trim().Length > 200)
        {
            errors.Add("Test title cannot be longer than 200 characters.");
        }

        if (request.Description?.Length > 4000)
        {
            errors.Add("Test description cannot be longer than 4000 characters.");
        }

        if (request.Questions.Count == 0)
        {
            errors.Add("Test must contain at least one question.");
        }

        for (var questionIndex = 0; questionIndex < request.Questions.Count; questionIndex++)
        {
            ValidateQuestion(request.Questions[questionIndex], questionIndex, errors);
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static void ValidateQuestion(
        QuestionWriteDto question,
        int questionIndex,
        ICollection<string> errors)
    {
        var questionNumber = questionIndex + 1;

        if (string.IsNullOrWhiteSpace(question.Text))
        {
            errors.Add($"Question {questionNumber}: text is required.");
        }

        if (!Enum.IsDefined(question.Type))
        {
            errors.Add($"Question {questionNumber}: unsupported question type.");
        }

        if (question.Options.Count < 2)
        {
            errors.Add($"Question {questionNumber}: at least two answer options are required.");
        }

        var correctCount = 0;
        for (var optionIndex = 0; optionIndex < question.Options.Count; optionIndex++)
        {
            var option = question.Options[optionIndex];
            if (string.IsNullOrWhiteSpace(option.Text))
            {
                errors.Add($"Question {questionNumber}, option {optionIndex + 1}: text is required.");
            }

            if (option.IsCorrect)
            {
                correctCount++;
            }
        }

        if (question.Type == QuestionType.SingleChoice && correctCount != 1)
        {
            errors.Add($"Question {questionNumber}: single choice questions must have exactly one correct option.");
        }

        if (question.Type == QuestionType.MultipleChoice && correctCount == 0)
        {
            errors.Add($"Question {questionNumber}: multiple choice questions must have at least one correct option.");
        }
    }
}
