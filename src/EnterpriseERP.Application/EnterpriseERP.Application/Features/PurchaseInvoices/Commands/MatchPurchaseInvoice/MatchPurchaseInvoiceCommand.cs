using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.PurchaseInvoices.Commands.MatchPurchaseInvoice;

public record MatchPurchaseInvoiceCommand(Guid PurchaseInvoiceId) : IRequest<Result<bool>>;

public class MatchPurchaseInvoiceCommandHandler : IRequestHandler<MatchPurchaseInvoiceCommand, Result<bool>>
{
    private readonly IPurchaseMatchingService _matchingService;

    public MatchPurchaseInvoiceCommandHandler(IPurchaseMatchingService matchingService)
    {
        _matchingService = matchingService;
    }

    public async Task<Result<bool>> Handle(MatchPurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        return await _matchingService.PerformThreeWayMatchAsync(request.PurchaseInvoiceId);
    }
}
