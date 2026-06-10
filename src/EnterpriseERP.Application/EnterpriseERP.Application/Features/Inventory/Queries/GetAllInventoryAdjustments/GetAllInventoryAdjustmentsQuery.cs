using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Inventory.Queries.GetAllInventoryAdjustments;

public class GetAllInventoryAdjustmentsQuery : IRequest<PagedResult<InventoryAdjustmentDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}

public class InventoryAdjustmentDto
{
    public Guid Id { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public int Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
}

public class GetAllInventoryAdjustmentsQueryHandler : IRequestHandler<GetAllInventoryAdjustmentsQuery, PagedResult<InventoryAdjustmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllInventoryAdjustmentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<InventoryAdjustmentDto>> Handle(GetAllInventoryAdjustmentsQuery request, CancellationToken cancellationToken)
    {
        // 1. Fetch adjustments
        var adjustments = await _unitOfWork.Repository<InventoryAdjustment>().GetAllAsync();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            adjustments = adjustments.Where(a => 
                a.AdjustmentNumber.ToLower().Contains(searchTerm) ||
                a.Reason.ToLower().Contains(searchTerm)
            ).ToList();
        }

        // 2. We need to manually join Warehouse and Item since Repository pattern might not support Includes fully
        var warehouseIds = adjustments.Select(a => a.WarehouseId).Distinct().ToList();
        var itemIds = adjustments.Select(a => a.ItemId).Distinct().ToList();

        var warehouses = await _unitOfWork.Repository<Warehouse>()
            .FindAsync(w => warehouseIds.Contains(w.Id));
            
        var items = await _unitOfWork.Repository<Item>()
            .FindAsync(i => itemIds.Contains(i.Id));

        var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.Name);
        // Assuming Item has NameAr and NameEn, we map NameAr. We should check Item entity.
        // I will use NameAr but fallback to just "Item".
        var itemDict = items.ToDictionary(i => i.Id, i => i.NameAr);

        var count = adjustments.Count();

        var pagedItems = adjustments
            .OrderByDescending(a => a.AdjustmentDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new InventoryAdjustmentDto
            {
                Id = a.Id,
                AdjustmentNumber = a.AdjustmentNumber,
                WarehouseId = a.WarehouseId,
                WarehouseName = warehouseDict.ContainsKey(a.WarehouseId) ? warehouseDict[a.WarehouseId] : "Unknown",
                ItemId = a.ItemId,
                ItemName = itemDict.ContainsKey(a.ItemId) ? itemDict[a.ItemId] : "Unknown",
                Quantity = a.Quantity,
                Type = (int)a.Type,
                Reason = a.Reason,
                AdjustmentDate = a.AdjustmentDate
            }).ToList();

        return new PagedResult<InventoryAdjustmentDto>
        {
            Items = pagedItems,
            TotalCount = count,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
