using EnterpriseERP.SharedKernel.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseERP.SharedKernel.Behaviors;

/// <summary>
/// Catches DomainExceptions and re-throws them clearly,
/// ensuring they are logged distinctly from system exceptions.
/// Does NOT swallow the exception — controllers handle HTTP mapping.
/// </summary>
public class DomainExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<DomainExceptionBehavior<TRequest, TResponse>> _logger;

    public DomainExceptionBehavior(ILogger<DomainExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(
                "Domain rule violation in {RequestName}: {Message}",
                typeof(TRequest).Name,
                ex.Message);

            // Re-throw so the API layer can map it to HTTP 400/422
            throw;
        }
    }
}
