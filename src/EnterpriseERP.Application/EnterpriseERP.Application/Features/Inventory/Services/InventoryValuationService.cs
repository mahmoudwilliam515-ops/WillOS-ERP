using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.SharedKernel.Common;
using MediatR;

namespace EnterpriseERP.Application.Features.Inventory.Services;

/// <summary>
/// Domain service for computing item cost based on valuation method.
/// Supports FIFO and Weighted Average.
/// </summary>
public interface IInventoryValuationService
{
    Task<(decimal UnitCost, decimal TotalCost)> CalculateCostAsync(Guid itemId, Guid warehouseId, decimal quantityOut, CancellationToken cancellationToken = default);
    Task UpdateWeightedAverageCostAsync(Guid itemId, decimal purchaseQty, decimal purchaseCost, CancellationToken cancellationToken = default);
}

public class InventoryValuationService : IInventoryValuationService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryValuationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Calculates the unit cost for outgoing stock based on the item's valuation method.
    /// FIFO: Consumes the oldest batches first, returning a blended cost.
    /// WeightedAverage: Returns the item's current average cost directly.
    /// </summary>
    public async Task<(decimal UnitCost, decimal TotalCost)> CalculateCostAsync(Guid itemId, Guid warehouseId, decimal quantityOut, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Repository<Item>().GetByIdAsync(itemId);
        if (item == null)
            throw new InvalidOperationException($"Item {itemId} not found.");

        if (item.ValuationMethod == ItemValuationMethod.WeightedAverage)
        {
            return (item.AverageCost, item.AverageCost * quantityOut);
        }

        if (item.ValuationMethod == ItemValuationMethod.FIFO)
        {
            return await CalculateFifoCostAsync(itemId, warehouseId, quantityOut);
        }

        // Default fallback
        return (item.AverageCost, item.AverageCost * quantityOut);
    }

    private async Task<(decimal UnitCost, decimal TotalCost)> CalculateFifoCostAsync(Guid itemId, Guid warehouseId, decimal quantityOut)
    {
        var allTransactions = (await _unitOfWork.Repository<InventoryTransaction>()
            .FindAsync(t => t.ItemId == itemId && t.WarehouseId == warehouseId))
            .OrderBy(t => t.TransactionDate)
            .ToList();

        var inTransactions = allTransactions.Where(t => t.Quantity > 0).ToList();
        var outQuantityTotal = Math.Abs(allTransactions.Where(t => t.Quantity < 0).Sum(t => t.Quantity));

        // Adjust IN transactions by subtracting already consumed OUT quantity
        var availableBatches = new List<(decimal Quantity, decimal UnitCost)>();
        foreach (var txn in inTransactions)
        {
            if (outQuantityTotal >= txn.Quantity)
            {
                outQuantityTotal -= txn.Quantity; // Fully consumed
            }
            else if (outQuantityTotal > 0)
            {
                var remainingInTxn = txn.Quantity - outQuantityTotal;
                availableBatches.Add((remainingInTxn, txn.UnitCost));
                outQuantityTotal = 0; // Partial consumption
            }
            else
            {
                availableBatches.Add((txn.Quantity, txn.UnitCost));
            }
        }

        decimal remainingToCost = quantityOut;
        decimal totalCost = 0;

        foreach (var batch in availableBatches)
        {
            if (remainingToCost <= 0) break;

            var consumable = Math.Min(batch.Quantity, remainingToCost);
            totalCost += consumable * batch.UnitCost;
            remainingToCost -= consumable;
        }

        if (remainingToCost > 0)
        {
            // Not enough stock - use last known cost or 0
            var lastTxnUnitCost = availableBatches.LastOrDefault().UnitCost;
            if (lastTxnUnitCost == 0 && inTransactions.Any()) 
                lastTxnUnitCost = inTransactions.Last().UnitCost;
                
            totalCost += remainingToCost * lastTxnUnitCost;
        }

        var unitCost = quantityOut > 0 ? Math.Round(totalCost / quantityOut, 4) : 0;
        return (unitCost, totalCost);
    }

    /// <summary>
    /// Updates the item's Weighted Average Cost when a new purchase arrives.
    /// Formula: ((CurrentQty * CurrentAvgCost) + (NewQty * NewCost)) / (CurrentQty + NewQty)
    /// </summary>
    public async Task UpdateWeightedAverageCostAsync(Guid itemId, decimal purchaseQty, decimal purchaseCost, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Repository<Item>().GetByIdAsync(itemId);
        if (item == null) return;

        if (item.ValuationMethod != ItemValuationMethod.WeightedAverage) return;

        // Get current total stock quantity
        var allTransactions = await _unitOfWork.Repository<InventoryTransaction>()
            .FindAsync(t => t.ItemId == itemId);
        var currentQty = allTransactions.Sum(t => t.Quantity);

        // Before this purchase (subtract new purchase)
        var prevQty = currentQty - purchaseQty;

        if (prevQty + purchaseQty > 0)
        {
            var newAvgCost = ((prevQty * item.AverageCost) + (purchaseQty * purchaseCost)) / (prevQty + purchaseQty);
            item.AverageCost = Math.Round(newAvgCost, 4);
            item.LastBuyPrice = purchaseCost;
            _unitOfWork.Repository<Item>().Update(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
