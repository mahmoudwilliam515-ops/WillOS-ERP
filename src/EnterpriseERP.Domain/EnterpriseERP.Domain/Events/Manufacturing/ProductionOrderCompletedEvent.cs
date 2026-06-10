using EnterpriseERP.SharedKernel.DomainEvents;
using System;

namespace EnterpriseERP.Domain.Events.Manufacturing;

public record ProductionOrderCompletedEvent : IDomainEvent
{
    public Guid ProductionOrderId { get; init; }
    public Guid ProductId { get; init; }
    public decimal ProducedQuantity { get; init; }
    public decimal TotalCost { get; init; }
    public decimal TotalMaterialCost { get; init; }
    public decimal TotalLaborCost { get; init; }
    public Guid WarehouseId { get; init; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public ProductionOrderCompletedEvent(Guid productionOrderId, Guid productId, decimal producedQuantity, decimal totalCost, decimal totalMaterialCost, decimal totalLaborCost, Guid warehouseId)
    {
        ProductionOrderId = productionOrderId;
        ProductId = productId;
        ProducedQuantity = producedQuantity;
        TotalCost = totalCost;
        TotalMaterialCost = totalMaterialCost;
        TotalLaborCost = totalLaborCost;
        WarehouseId = warehouseId;
    }
}

