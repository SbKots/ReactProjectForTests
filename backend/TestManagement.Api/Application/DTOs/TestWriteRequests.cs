using TestManagement.Api.Domain.Enums;

namespace TestManagement.Api.Application.DTOs;

public abstract class TestWriteRequest
{
    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public IReadOnlyList<QuestionWriteDto> Questions { get; init; } = [];
}

public sealed class CreateTestRequest : TestWriteRequest;

public sealed class UpdateTestRequest : TestWriteRequest;

public sealed class QuestionWriteDto
{
    public string Text { get; init; } = string.Empty;

    public QuestionType Type { get; init; }

    public IReadOnlyList<AnswerOptionWriteDto> Options { get; init; } = [];
}

public sealed class AnswerOptionWriteDto
{
    public string Text { get; init; } = string.Empty;

    public bool IsCorrect { get; init; }
}

public sealed class SubmitTestRequest
{
    public IReadOnlyList<QuestionAnswerSubmissionDto> Answers { get; init; } = [];
}

public sealed class QuestionAnswerSubmissionDto
{
    public Guid QuestionId { get; init; }

    public IReadOnlyList<Guid> SelectedOptionIds { get; init; } = [];
}
