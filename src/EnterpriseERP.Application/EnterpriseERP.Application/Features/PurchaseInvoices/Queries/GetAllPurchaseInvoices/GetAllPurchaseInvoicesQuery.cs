using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Application.Features.PurchaseInvoices.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseInvoices.Queries.GetAllPurchaseInvoices;

public class GetAllPurchaseInvoicesQuery : IRequest<PagedResult<PurchaseInvoiceDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SearchTerm { get; set; }
}
