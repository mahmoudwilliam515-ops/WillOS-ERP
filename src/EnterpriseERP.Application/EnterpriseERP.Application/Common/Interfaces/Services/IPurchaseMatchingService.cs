using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.SharedKernel.Results;
using System;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IPurchaseMatchingService
{
    Task<Result<bool>> PerformThreeWayMatchAsync(Guid purchaseInvoiceId);
    Task<(bool Success, string Message, PurchaseInvoiceStatus ResultStatus)> MatchInvoiceAsync(PurchaseInvoice invoice);
}
