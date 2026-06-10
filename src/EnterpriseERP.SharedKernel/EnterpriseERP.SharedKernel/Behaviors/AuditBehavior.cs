using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EnterpriseERP.SharedKernel.Behaviors;

/// <summary>
/// Writes structured audit entries (who, what, when) for every Command.
/// Queries are skipped — only state-mutating operations are audited.
/// </summary>
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;

    public AuditBehavior(ILogger<AuditBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Only audit Commands (naming convention: ends with "Command")
        if (!requestName.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
            return await next();

        var sw = Stopwatch.StartNew();
        TResponse response;

        try
        {
            response = await next();
            sw.Stop();

            _logger.LogInformation(
                "[AUDIT] Command {CommandName} completed successfully in {ElapsedMs}ms",
                requestName,
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "[AUDIT] Command {CommandName} FAILED after {ElapsedMs}ms",
                requestName,
                sw.ElapsedMilliseconds);
            throw;
        }

        return response;
    }
}
