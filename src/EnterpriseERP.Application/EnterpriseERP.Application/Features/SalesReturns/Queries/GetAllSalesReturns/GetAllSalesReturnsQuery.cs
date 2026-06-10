using EnterpriseERP.Application.Features.SalesReturns.DTOs;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.SalesReturns.Queries.GetAllSalesReturns;

public class GetAllSalesReturnsQuery : IRequest<PagedResult<SalesReturnDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
}
