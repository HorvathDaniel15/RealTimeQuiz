using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RealTimeQuiz.Logic.Exceptions;

namespace RealTimeQuiz.WebAPI.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteProblemDetailsAsync(context, ex);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            BusinessValidationException => (StatusCodes.Status400BadRequest, "Business validation failed."),
            EntityNotFoundException => (StatusCodes.Status404NotFound, "Entity not found."),
            ForbiddenOperationException => (StatusCodes.Status403Forbidden, "Operation is forbidden."),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error.")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = ex.Message,
            Type = $"https://httpstatuses.com/{status}",
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

        if (ex is BusinessValidationException validationEx)
        {
            problem.Extensions["errors"] = validationEx.Errors;
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem);
    }
}