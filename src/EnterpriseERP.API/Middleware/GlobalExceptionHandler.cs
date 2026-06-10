using EnterpriseERP.SharedKernel.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EnterpriseERP.API.Middleware;

/// <summary>
/// Global exception handler — maps well-known domain/validation exceptions
/// to structured HTTP Problem Details responses.
/// Registered via app.UseExceptionHandler() in Program.cs.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        (int statusCode, string title, IEnumerable<string> errors) = exception switch
        {
            ValidationException ve => (
                (int)HttpStatusCode.UnprocessableEntity,
                "Validation Failed",
                ve.Errors.Select(e => e.ErrorMessage)),

            DomainException de => (
                (int)HttpStatusCode.BadRequest,
                "Business Rule Violation",
                new[] { de.Message }),

            KeyNotFoundException => (
                (int)HttpStatusCode.NotFound,
                "Resource Not Found",
                new[] { exception.Message }),

            UnauthorizedAccessException => (
                (int)HttpStatusCode.Forbidden,
                "Forbidden",
                new[] { exception.Message }),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                "Internal Server Error",
                new[] { "An unexpected error occurred." })
        };

        // Log at appropriate level
        if (statusCode == (int)HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning("Handled exception [{StatusCode}] {Title}: {Message}", statusCode, title, exception.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{statusCode}",
            Extensions = { ["errors"] = errors }
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
