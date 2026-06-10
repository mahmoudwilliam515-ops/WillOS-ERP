using MediatR;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.SharedKernel.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(IUnitOfWork unitOfWork, ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only apply transaction to Commands (requests that don't start with "Get")
        var requestName = typeof(TRequest).Name;
        if (requestName.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
        {
            return await next();
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            _logger.LogInformation("Beginning transaction for {RequestName}", requestName);

            var response = await next();

            if (response.IsSuccess)
            {
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Committed transaction for {RequestName}", requestName);
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning("Rolled back transaction for {RequestName} due to failure: {Error}", requestName, response.Error);
            }

            return response;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Transaction failed for {RequestName}", requestName);
            throw;
        }
    }
}
