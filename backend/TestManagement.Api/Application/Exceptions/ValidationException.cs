namespace TestManagement.Api.Application.Exceptions;

public sealed class ValidationException(IReadOnlyList<string> errors) : Exception("Validation failed.")
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
