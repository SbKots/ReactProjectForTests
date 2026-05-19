using TestManagement.Api.Domain.Enums;

namespace TestManagement.Api.Domain.Entities;

public sealed class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TestDefinitionId { get; set; }

    public TestDefinition TestDefinition { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    public QuestionType Type { get; set; }

    public int SortOrder { get; set; }

    public List<AnswerOption> Options { get; set; } = [];
}
