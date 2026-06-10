using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Inventory.DTOs;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;

namespace EnterpriseERP.Application.Features.Inventory.Queries.GetStockBalance;

public class GetStockBalanceQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetStockBalanceQuery, IEnumerable<StockBalanceDto>>
{
    public Task<IEnumerable<StockBalanceDto>> Handle(GetStockBalanceQuery request, CancellationToken cancellationToken)
    {
        var query = unitOfWork.Repository<InventoryTransaction>().Query();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(t => t.WarehouseId == request.WarehouseId.Value);
        }

        if (request.ItemId.HasValue)
        {
            query = query.Where(t => t.ItemId == request.ItemId.Value);
        }

        var balances = query
            .GroupBy(t => new { t.WarehouseId, WarehouseName = t.Warehouse.Name, t.ItemId, ItemName = t.Item.NameAr, ItemCode = t.Item.Code })
            .Select(g => new StockBalanceDto
            {
                WarehouseId = g.Key.WarehouseId,
                WarehouseName = g.Key.WarehouseName,
                ItemId = g.Key.ItemId,
                ItemName = g.Key.ItemName,
                ItemCode = g.Key.ItemCode,
                TotalIn = g.Sum(x => x.Type == TransactionType.PurchaseIn || x.Type == TransactionType.TransferIn || x.Type == TransactionType.AdjustmentIn || x.Type == TransactionType.ReturnFromCustomer ? x.Quantity : 0),
                TotalOut = g.Sum(x => x.Type == TransactionType.SalesOut || x.Type == TransactionType.TransferOut || x.Type == TransactionType.AdjustmentOut || x.Type == TransactionType.ReturnToSupplier ? x.Quantity : 0)
            })
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        foreach (var balance in balances)
        {
            balance.CurrentBalance = balance.TotalIn - balance.TotalOut;
        }

        return Task.FromResult(balances.AsEnumerable());
    }
}
