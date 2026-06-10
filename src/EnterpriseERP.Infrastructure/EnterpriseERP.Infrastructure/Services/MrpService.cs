using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Infrastructure.Services;

public class MrpService : IMrpService
{
    private readonly IUnitOfWork _unitOfWork;

    public MrpService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<MrpRecommendationDto>> RunMrpAsync(Guid companyId, DateTime forecastEndDate, CancellationToken cancellationToken = default)
    {
        var recommendations = new List<MrpRecommendationDto>();

        // 1. Get Demand from Sales Orders (Approved but not yet fulfilled)
        // جلب SalesOrder IDs النشطة أولاً ثم Query الـ Lines
        var activeSalesOrderIds = await _unitOfWork.Repository<EnterpriseERP.Domain.Sales.SalesOrder>().Query()
            .Where(o => o.Status == EnterpriseERP.Domain.Sales.SalesOrderStatus.Confirmed
                     || o.Status == EnterpriseERP.Domain.Sales.SalesOrderStatus.Shipped
                     || o.Status == EnterpriseERP.Domain.Sales.SalesOrderStatus.Shipped)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        var salesDemand = await _unitOfWork.Repository<EnterpriseERP.Domain.Sales.SalesOrderLine>().Query()
            .Where(l => activeSalesOrderIds.Contains(l.SalesOrderId))
            .GroupBy(l => l.ItemId)
            .Select(g => new { ItemId = g.Key, Quantity = g.Sum(l => l.OrderedQuantity) })
            .ToListAsync(cancellationToken);

        // 2. Get Demand from existing Production Orders (Materials needed)
        var productionDemand = await _unitOfWork.Repository<ProductionOrderMaterial>().Query()
            .Where(m => m.ProductionOrder.Status != ProductionOrderStatus.Completed &&
                        m.ProductionOrder.Status != ProductionOrderStatus.Cancelled)
            .GroupBy(m => m.RawMaterialId)
            .Select(g => new { ItemId = g.Key, Quantity = g.Sum(m => m.PlannedQuantity - m.ActualQuantity) })
            .ToListAsync(cancellationToken);

        // 3. Get Supply from Open Purchase Orders
        var purchaseSupply = await _unitOfWork.Repository<PurchaseOrderLine>().Query()
            .Where(l => l.PurchaseOrder.Status != PurchaseOrderStatus.Received &&
                        l.PurchaseOrder.Status != PurchaseOrderStatus.Cancelled)
            .GroupBy(l => l.ItemId)
            .Select(g => new { ItemId = g.Key, Quantity = g.Sum(l => l.OrderedQuantity) })
            .ToListAsync(cancellationToken);

        // Merge Demands and Supplies
        var itemIds = salesDemand.Select(d => d.ItemId)
            .Union(productionDemand.Select(p => p.ItemId))
            .Union(purchaseSupply.Select(s => s.ItemId))
            .Distinct();

        // 4. Process each item
        foreach (var itemId in itemIds)
        {
            var item = await _unitOfWork.Repository<Item>().GetByIdAsync(itemId);
            if (item == null) continue;

            var demandQty = salesDemand.Where(d => d.ItemId == itemId).Sum(d => d.Quantity) +
                            productionDemand.Where(p => p.ItemId == itemId).Sum(p => p.Quantity);
            
            var supplyQty = purchaseSupply.Where(s => s.ItemId == itemId).Sum(s => s.Quantity);

            // Check current stock levels
            var currentStock = await _unitOfWork.Repository<InventoryTransaction>().Query()
                .Where(t => t.ItemId == itemId)
                .SumAsync(t => t.Quantity, cancellationToken);

            var projectedBalance = currentStock + supplyQty - demandQty;

            // If projected balance is below safety stock
            if (projectedBalance < item.MinStock)
            {
                var netRequirement = item.MinStock - projectedBalance;
                var suggestedQty = Math.Max(netRequirement, 10); 
                
                var bom = await _unitOfWork.Repository<BillOfMaterials>().Query()
                    .FirstOrDefaultAsync(b => b.ProductId == itemId && b.IsDefault, cancellationToken);

                recommendations.Add(new MrpRecommendationDto
                {
                    ItemId = itemId,
                    ItemCode = item.Code,
                    ItemName = item.Name,
                    RequiredQuantity = demandQty,
                    CurrentStock = currentStock,
                    SuggestedOrderQuantity = suggestedQty,
                    LeadTimeDays = item.LeadTimeDays,
                    SuggestedDate = DateTime.UtcNow.AddDays(item.LeadTimeDays),
                    Type = bom != null ? RecommendationType.ProductionOrder : RecommendationType.PurchaseOrder,
                    Reason = projectedBalance < 0 ? "Out of Stock" : "Below Safety Stock"
                });
            }
        }

        return recommendations;
    }
}





