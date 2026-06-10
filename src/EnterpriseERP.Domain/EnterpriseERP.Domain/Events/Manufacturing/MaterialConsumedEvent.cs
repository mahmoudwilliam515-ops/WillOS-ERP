using EnterpriseERP.SharedKernel.DomainEvents;
using System;

namespace EnterpriseERP.Domain.Events.Manufacturing;

public record MaterialConsumedEvent : IDomainEvent
{
    public Guid ProductionOrderId { get; init; }
    public Guid MaterialId { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public Guid WarehouseId { get; init; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public MaterialConsumedEvent(Guid productionOrderId, Guid materialId, decimal quantity, decimal unitCost, Guid warehouseId)
    {
        ProductionOrderId = productionOrderId;
        MaterialId = materialId;
        Quantity = quantity;
        UnitCost = unitCost;
        WarehouseId = warehouseId;
    }
}
