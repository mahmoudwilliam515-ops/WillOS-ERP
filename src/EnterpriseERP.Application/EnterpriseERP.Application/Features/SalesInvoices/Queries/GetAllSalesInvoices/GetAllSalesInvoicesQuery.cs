using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Application.Features.SalesInvoices.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.SalesInvoices.Queries.GetAllSalesInvoices;

public class GetAllSalesInvoicesQuery : IRequest<PagedResult<SalesInvoiceDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SearchTerm { get; set; }
}
