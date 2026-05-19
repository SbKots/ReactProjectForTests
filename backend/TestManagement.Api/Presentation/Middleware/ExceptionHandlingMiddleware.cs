using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TestManagement.Api.Application.Exceptions;

namespace TestManagement.Api.Presentation.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Validation error",
                exception.Message,
                exception.Errors);
        }
        catch (NotFoundException exception)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status404NotFound,
                "Resource not found",
                exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API exception.");

            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Server error",
                "Unexpected server error.");
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail,
        IReadOnlyList<string>? errors = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
