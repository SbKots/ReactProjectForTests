using TestManagement.Api.Domain.Enums;

namespace TestManagement.Api.Application.DTOs;

public sealed record TestSummaryDto(
    Guid Id,
    string Title,
    string? Description,
    int QuestionCount,
    DateTime UpdatedAtUtc);

public sealed record TestDetailsDto(
    Guid Id,
    string Title,
    string? Description,
    IReadOnlyList<QuestionDto> Questions,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record QuestionDto(
    Guid Id,
    string Text,
    QuestionType Type,
    IReadOnlyList<AnswerOptionDto> Options);

public sealed record AnswerOptionDto(
    Guid Id,
    string Text,
    bool IsCorrect);

public sealed record TestForTakingDto(
    Guid Id,
    string Title,
    string? Description,
    IReadOnlyList<QuestionForTakingDto> Questions);

public sealed record QuestionForTakingDto(
    Guid Id,
    string Text,
    QuestionType Type,
    IReadOnlyList<AnswerOptionForTakingDto> Options);

public sealed record AnswerOptionForTakingDto(
    Guid Id,
    string Text);

public sealed record TestSubmissionResultDto(
    Guid TestId,
    decimal Score,
    int MaxScore,
    decimal Percentage,
    IReadOnlyList<QuestionResultDto> Questions);

public sealed record QuestionResultDto(
    Guid QuestionId,
    decimal Score,
    decimal MaxScore,
    IReadOnlyList<Guid> CorrectOptionIds,
    IReadOnlyList<Guid> SelectedOptionIds);
