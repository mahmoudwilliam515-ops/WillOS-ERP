using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.SalesReturns.DTOs;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.SalesReturns.Queries.GetAllSalesReturns;

public class GetAllSalesReturnsQueryHandler : IRequestHandler<GetAllSalesReturnsQuery, PagedResult<SalesReturnDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSalesReturnsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<SalesReturnDto>> Handle(GetAllSalesReturnsQuery request, CancellationToken cancellationToken)
    {
        var search = request.SearchTerm?.Trim();
        var (items, totalCount) = await _unitOfWork.Repository<SalesReturn>().GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            string.IsNullOrWhiteSpace(search)
                ? null
                : r => r.ReturnNumber.Contains(search) || r.Reason.Contains(search));

        var list = items.ToList();
        var customerIds = list.Select(r => r.CustomerId).Distinct().ToList();
        var customers = customerIds.Count > 0
            ? (await _unitOfWork.Repository<Customer>().FindAsync(c => customerIds.Contains(c.Id)))
                .ToDictionary(c => c.Id, c => c.Name)
            : new Dictionary<Guid, string>();

        var dtos = list.Select(r => new SalesReturnDto
        {
            Id = r.Id,
            ReturnNumber = r.ReturnNumber,
            ReturnDate = r.ReturnDate,
            CustomerId = r.CustomerId,
            CustomerName = customers.TryGetValue(r.CustomerId, out var name) ? name : string.Empty,
            TotalAmount = r.TotalAmount,
            Reason = r.Reason,
            Status = (int)r.Status,
            StatusName = r.Status.ToString()
        });

        return new PagedResult<SalesReturnDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
