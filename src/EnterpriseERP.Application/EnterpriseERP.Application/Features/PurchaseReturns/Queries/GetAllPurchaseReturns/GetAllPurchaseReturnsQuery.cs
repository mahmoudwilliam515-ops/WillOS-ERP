using EnterpriseERP.Application.Features.PurchaseReturns.DTOs;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseReturns.Queries.GetAllPurchaseReturns;

public class GetAllPurchaseReturnsQuery : IRequest<PagedResult<PurchaseReturnDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
}
