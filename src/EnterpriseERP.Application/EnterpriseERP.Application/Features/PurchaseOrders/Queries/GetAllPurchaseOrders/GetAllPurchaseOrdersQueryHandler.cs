using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;

public class GetAllPurchaseOrdersQueryHandler : IRequestHandler<GetAllPurchaseOrdersQuery, PagedResult<PurchaseOrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllPurchaseOrdersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<PurchaseOrderDto>> Handle(GetAllPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Repository<PurchaseOrder>()
            .GetPagedAsync(request.PageNumber, request.PageSize,
                string.IsNullOrWhiteSpace(request.SearchTerm)
                    ? null
                    : q => q.OrderNumber.Contains(request.SearchTerm));

        var purchaseOrders = items.ToList();
        var orderIds = purchaseOrders.Select(q => q.Id).ToHashSet();
        var lines = orderIds.Count > 0
            ? await _unitOfWork.Repository<PurchaseOrderLine>().FindAsync(l => orderIds.Contains(l.PurchaseOrderId))
            : Enumerable.Empty<PurchaseOrderLine>();
        var lineCounts = lines.GroupBy(l => l.PurchaseOrderId).ToDictionary(g => g.Key, g => g.Count());

        var dtos = purchaseOrders.Select(q => new PurchaseOrderDto
        {
            Id = q.Id,
            OrderNumber = q.OrderNumber,
            OrderDate = q.OrderDate,
            ExpectedDelivery = q.ExpectedDeliveryDate,
            SupplierName = q.SupplierId.ToString(),
            LineCount = lineCounts.GetValueOrDefault(q.Id),
            TotalAmount = q.TotalAmount,
            Status = q.Status.ToString()
        });

        return new PagedResult<PurchaseOrderDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
