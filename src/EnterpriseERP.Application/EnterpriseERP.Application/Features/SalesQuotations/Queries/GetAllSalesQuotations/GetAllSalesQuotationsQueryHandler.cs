using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.SalesQuotations.Queries.GetAllSalesQuotations;

public class GetAllSalesQuotationsQueryHandler : IRequestHandler<GetAllSalesQuotationsQuery, PagedResult<SalesQuotationDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSalesQuotationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<SalesQuotationDto>> Handle(GetAllSalesQuotationsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Repository<SalesQuotation>()
            .GetPagedAsync(request.PageNumber, request.PageSize,
                string.IsNullOrWhiteSpace(request.SearchTerm)
                    ? null
                    : q => q.QuotationNumber.Contains(request.SearchTerm));

        var dtos = items.Select(q => new SalesQuotationDto
        {
            Id = q.Id,
            QuotationNumber = q.QuotationNumber,
            QuotationDate = q.QuotationDate,
            ValidUntil = q.ValidUntil,
            CustomerName = q.CustomerId.ToString(), // Resolved at UI level or via separate lookup
            TotalAmount = q.TotalAmount,
            Status = q.Status.ToString()
        });

        return new PagedResult<SalesQuotationDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
