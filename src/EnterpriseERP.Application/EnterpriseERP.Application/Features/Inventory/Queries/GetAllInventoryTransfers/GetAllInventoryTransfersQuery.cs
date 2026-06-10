using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Inventory.Queries.GetAllInventoryTransfers;

public class GetAllInventoryTransfersQuery : IRequest<PagedResult<InventoryTransferDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}

public class InventoryTransferDto
{
    public Guid Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public Guid FromWarehouseId { get; set; }
    public string FromWarehouseName { get; set; } = string.Empty;
    public Guid ToWarehouseId { get; set; }
    public string ToWarehouseName { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public int Status { get; set; }
    public string Remarks { get; set; } = string.Empty;
}

public class GetAllInventoryTransfersQueryHandler : IRequestHandler<GetAllInventoryTransfersQuery, PagedResult<InventoryTransferDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllInventoryTransfersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<InventoryTransferDto>> Handle(GetAllInventoryTransfersQuery request, CancellationToken cancellationToken)
    {
        var transfers = await _unitOfWork.Repository<InventoryTransfer>().GetAllAsync();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            transfers = transfers.Where(t => 
                t.TransferNumber.ToLower().Contains(searchTerm) ||
                t.Remarks.ToLower().Contains(searchTerm)
            ).ToList();
        }

        var warehouseIds = transfers.Select(t => t.FromWarehouseId)
            .Concat(transfers.Select(t => t.ToWarehouseId))
            .Distinct()
            .ToList();

        var warehouses = await _unitOfWork.Repository<Warehouse>()
            .FindAsync(w => warehouseIds.Contains(w.Id));

        var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.Name);

        var count = transfers.Count();

        var pagedItems = transfers
            .OrderByDescending(t => t.TransferDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new InventoryTransferDto
            {
                Id = t.Id,
                TransferNumber = t.TransferNumber,
                FromWarehouseId = t.FromWarehouseId,
                FromWarehouseName = warehouseDict.ContainsKey(t.FromWarehouseId) ? warehouseDict[t.FromWarehouseId] : "Unknown",
                ToWarehouseId = t.ToWarehouseId,
                ToWarehouseName = warehouseDict.ContainsKey(t.ToWarehouseId) ? warehouseDict[t.ToWarehouseId] : "Unknown",
                TransferDate = t.TransferDate,
                Status = (int)t.Status,
                Remarks = t.Remarks
            }).ToList();

        return new PagedResult<InventoryTransferDto>
        {
            Items = pagedItems,
            TotalCount = count,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
