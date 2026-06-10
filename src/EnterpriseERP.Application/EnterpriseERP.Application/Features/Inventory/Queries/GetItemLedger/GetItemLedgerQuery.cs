using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Application.Features.Inventory.DTOs;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;

namespace EnterpriseERP.Application.Features.Inventory.Queries.GetItemLedger;

public record GetItemLedgerQuery : IRequest<PagedResult<ItemLedgerDto>>
{
    public Guid ItemId { get; init; }
    public Guid? WarehouseId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public class GetItemLedgerQueryHandler : IRequestHandler<GetItemLedgerQuery, PagedResult<ItemLedgerDto>>
{
    private readonly IGenericRepository<InventoryTransaction> _transactionRepo;
    private readonly IGenericRepository<Warehouse> _warehouseRepo;

    public GetItemLedgerQueryHandler(
        IGenericRepository<InventoryTransaction> transactionRepo,
        IGenericRepository<Warehouse> warehouseRepo)
    {
        _transactionRepo = transactionRepo;
        _warehouseRepo = warehouseRepo;
    }

    public async Task<PagedResult<ItemLedgerDto>> Handle(GetItemLedgerQuery request, CancellationToken cancellationToken)
    {
        // Get all transactions for this item to calculate running balance
        var allTransactions = await _transactionRepo.FindAsync(t => 
            t.ItemId == request.ItemId &&
            (!request.WarehouseId.HasValue || t.WarehouseId == request.WarehouseId.Value));

        var sortedTransactions = allTransactions.OrderBy(t => t.TransactionDate).ToList();

        // Calculate running balances
        var ledgerLines = new List<ItemLedgerDto>();
        decimal runningBalance = 0;

        var warehouseIds = sortedTransactions.Select(t => t.WarehouseId).Distinct().ToList();
        var warehouses = await _warehouseRepo.FindAsync(w => warehouseIds.Contains(w.Id));
        var warehouseMap = warehouses.ToDictionary(w => w.Id, w => w.Name);

        foreach (var t in sortedTransactions)
        {
            var isOut = t.Quantity < 0;
            var qtyIn = isOut ? 0 : t.Quantity;
            var qtyOut = isOut ? Math.Abs(t.Quantity) : 0;
            
            // Adjust running balance
            runningBalance += t.Quantity;

            // Only add to result if it matches date filters
            bool include = true;
            if (request.FromDate.HasValue && t.TransactionDate < request.FromDate.Value) include = false;
            if (request.ToDate.HasValue && t.TransactionDate > request.ToDate.Value) include = false;

            if (include)
            {
                ledgerLines.Add(new ItemLedgerDto
                {
                    TransactionId = t.Id,
                    TransactionDate = t.TransactionDate,
                    TransactionType = t.Type.ToString(),
                    ReferenceNumber = t.ReferenceNumber ?? "",
                    QuantityIn = qtyIn,
                    QuantityOut = qtyOut,
                    RunningBalance = runningBalance,
                    WarehouseName = warehouseMap.GetValueOrDefault(t.WarehouseId, "Unknown"),
                    Notes = t.Notes
                });
            }
        }

        // Apply pagination in memory since we calculated running balance sequentially
        var totalCount = ledgerLines.Count;
        var pagedItems = ledgerLines
            .OrderByDescending(l => l.TransactionDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<ItemLedgerDto>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
