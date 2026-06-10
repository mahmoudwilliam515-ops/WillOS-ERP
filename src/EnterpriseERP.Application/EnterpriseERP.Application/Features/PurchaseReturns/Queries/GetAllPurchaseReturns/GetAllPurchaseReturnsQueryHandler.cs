using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.PurchaseReturns.DTOs;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseReturns.Queries.GetAllPurchaseReturns;

public class GetAllPurchaseReturnsQueryHandler : IRequestHandler<GetAllPurchaseReturnsQuery, PagedResult<PurchaseReturnDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllPurchaseReturnsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<PurchaseReturnDto>> Handle(GetAllPurchaseReturnsQuery request, CancellationToken cancellationToken)
    {
        var search = request.SearchTerm?.Trim();
        var (items, totalCount) = await _unitOfWork.Repository<PurchaseReturn>().GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            string.IsNullOrWhiteSpace(search)
                ? null
                : r => r.ReturnNumber.Contains(search) || r.Reason.Contains(search));

        var list = items.ToList();
        var supplierIds = list.Select(r => r.SupplierId).Distinct().ToList();
        var suppliers = supplierIds.Count > 0
            ? (await _unitOfWork.Repository<Supplier>().FindAsync(s => supplierIds.Contains(s.Id)))
                .ToDictionary(s => s.Id, s => s.Name)
            : new Dictionary<Guid, string>();

        var dtos = list.Select(r => new PurchaseReturnDto
        {
            Id = r.Id,
            ReturnNumber = r.ReturnNumber,
            ReturnDate = r.ReturnDate,
            SupplierId = r.SupplierId,
            SupplierName = suppliers.TryGetValue(r.SupplierId, out var name) ? name : string.Empty,
            TotalAmount = r.TotalAmount,
            Reason = r.Reason,
            Status = (int)r.Status,
            StatusName = r.Status.ToString()
        });

        return new PagedResult<PurchaseReturnDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
