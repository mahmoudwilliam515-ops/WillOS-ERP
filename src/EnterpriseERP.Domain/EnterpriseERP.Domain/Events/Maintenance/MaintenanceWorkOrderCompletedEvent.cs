using EnterpriseERP.SharedKernel.DomainEvents;
using System;

namespace EnterpriseERP.Domain.Events.Maintenance;

public record MaintenanceWorkOrderCompletedEvent : IDomainEvent
{
    public Guid WorkOrderId { get; init; }
    public Guid AssetId { get; init; }
    public decimal TotalCost { get; init; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public MaintenanceWorkOrderCompletedEvent(Guid workOrderId, Guid assetId, decimal totalCost)
    {
        WorkOrderId = workOrderId;
        AssetId = assetId;
        TotalCost = totalCost;
    }
}
