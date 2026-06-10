using EnterpriseERP.SharedKernel.Exceptions;
using EnterpriseERP.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EnterpriseERP.API.Middleware;

/// <summary>
/// Global exception handler — maps all well-known domain and validation exceptions
/// to structured RFC 7807 Problem Details responses (Phase 9 §9.3).
///
/// Mapping table:
///   FluentValidation.ValidationException  → 422  ERR_VALIDATION_FAILED
///   ConflictException                     → 409  ex.ErrorCode  (e.g. ERR_CONFLICT_INVALID_STATE)
///   DomainException                       → 400  ERR_BUSINESS_RULE_VIOLATION
///   KeyNotFoundException                  → 404  ERR_NOT_FOUND
///   UnauthorizedAccessException           → 403  ERR_FORBIDDEN
///   Everything else                       → 500  ERR_INTERNAL_UNEXPECTED (no detail exposed)
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception   exception,
        CancellationToken cancellationToken)
    {
        // ── Status code + title ──────────────────────────────────────────────
        (int statusCode, string title) = exception switch
        {
            ValidationException         => (422, "Validation Failed"),
            InsufficientInventoryException => (422, "Insufficient Inventory"),
            FIFOValuationException      => (422, "FIFO Valuation Error"),
            ConflictException           => (409, "Conflict"),
            DomainException             => (400, "Business Rule Violation"),
            KeyNotFoundException        => (404, "Resource Not Found"),
            UnauthorizedAccessException => (403, "Forbidden"),
            _                           => (500, "Internal Server Error")
        };

        // ── Machine-readable error_code ──────────────────────────────────────
        string errorCode = exception switch
        {
            ValidationException         => "ERR_VALIDATION_FAILED",
            InsufficientInventoryException => "ERR_INSUFFICIENT_INVENTORY",
            FIFOValuationException      => "ERR_FIFO_VALUATION_ERROR",
            ConflictException ce        => ce.ErrorCode,
            DomainException             => "ERR_BUSINESS_RULE_VIOLATION",
            KeyNotFoundException        => "ERR_NOT_FOUND",
            UnauthorizedAccessException => "ERR_FORBIDDEN",
            _                           => "ERR_INTERNAL_UNEXPECTED"
        };

        // ── Log at correct level ─────────────────────────────────────────────
        if (statusCode == 500)
            _logger.LogError(exception, "Unhandled exception [{RequestId}]",
                httpContext.TraceIdentifier);
        else
            _logger.LogWarning("Handled {StatusCode} [{ErrorCode}] {Message}",
                statusCode, errorCode, exception.Message);

        // ── Build RFC 7807 response ──────────────────────────────────────────
        // For 500 we never expose internal detail (§9.0.2 RFC 7807 EVERYWHERE)
        var detail = statusCode == 500
            ? "An unexpected error occurred. Quote request_id when contacting support."
            : exception.Message;

        var problem = new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Type     = $"https://api.erpplatform.com/errors/{errorCode.ToLower().Replace("_", "-")}",
            Detail   = detail,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["error_code"] = errorCode;
        problem.Extensions["request_id"] = httpContext.TraceIdentifier;

        // Validation errors — add structured errors[] array (§9.3.1)
        if (exception is ValidationException vEx)
        {
            problem.Extensions["errors"] = vEx.Errors
                .Select(e => new
                {
                    field   = e.PropertyName,
                    code    = e.ErrorCode,
                    message = e.ErrorMessage
                });
        }

        httpContext.Response.StatusCode  = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
