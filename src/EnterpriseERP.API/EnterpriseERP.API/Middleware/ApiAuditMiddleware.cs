using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Workflow;
using EnterpriseERP.Infrastructure.Data;

namespace EnterpriseERP.API.Middleware;

public class ApiAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiAuditMiddleware> _logger;

    public ApiAuditMiddleware(RequestDelegate next, ILogger<ApiAuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider, ICurrentUserService currentUserService)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid();

        // 1. Inject X-Request-ID
        if (!context.Request.Headers.TryGetValue("X-Request-ID", out var reqIdHeader) || !Guid.TryParse(reqIdHeader, out requestId))
        {
            requestId = Guid.NewGuid();
            context.Request.Headers["X-Request-ID"] = requestId.ToString();
        }
        context.Response.Headers["X-Request-ID"] = requestId.ToString();

        // 2. Hash Request Body (Async)
        string? requestBodyHash = null;
        if (context.Request.ContentLength > 0 && context.Request.Body.CanSeek)
        {
            context.Request.EnableBuffering();
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(context.Request.Body);
            requestBodyHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            context.Request.Body.Position = 0;
        }

        // 3. Hash Query Params
        string? queryParamsHash = null;
        if (context.Request.QueryString.HasValue)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(context.Request.QueryString.Value));
            queryParamsHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        // 4. Continue Request Pipeline
        await _next(context);

        stopwatch.Stop();

        // 5. Build Audit Log
        var companyId = currentUserService.CompanyId;
        if (companyId == null || companyId == Guid.Empty)
        {
            // Do not audit if tenant is not identified (e.g., anonymous routes like Swagger or health check)
            if (!context.Request.Path.Value!.StartsWith("/api/"))
                return;
            
            // For cross-tenant tracking, we assign an empty guid or log as a security warning.
            companyId = Guid.Empty;
        }

        var auditLog = new AuditApiLog
        {
            CompanyId = companyId.Value,
            UserId = string.IsNullOrEmpty(currentUserService.UserId) ? null : Guid.Parse(currentUserService.UserId),
            RequestId = requestId,
            HttpMethod = context.Request.Method,
            Endpoint = context.Request.Path.Value ?? string.Empty,
            QueryParamsHash = queryParamsHash,
            RequestBodyHash = requestBodyHash,
            ResponseStatus = (short)context.Response.StatusCode,
            LatencyMs = (int)stopwatch.ElapsedMilliseconds,
            IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            ApiVersion = context.Request.Path.Value?.Contains("/v1/") == true ? "v1" : (context.Request.Path.Value?.Contains("/v2/") == true ? "v2" : "Unknown"),
            OccurredAt = DateTimeOffset.UtcNow
        };

        // 6. Save Audit Log (Async / non-blocking using a background task or scope)
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.AuditApiLogs.Add(auditLog);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write API audit log to database for RequestId: {RequestId}", requestId);
            }
        });
    }
}
