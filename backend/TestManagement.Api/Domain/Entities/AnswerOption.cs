namespace TestManagement.Api.Domain.Entities;

public sealed class AnswerOption
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid QuestionId { get; set; }

    public Question Question { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public int SortOrder { get; set; }
}
