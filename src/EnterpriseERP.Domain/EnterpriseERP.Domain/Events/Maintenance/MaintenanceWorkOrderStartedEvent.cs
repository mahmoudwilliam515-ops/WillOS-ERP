using EnterpriseERP.SharedKernel.DomainEvents;
using System;

namespace EnterpriseERP.Domain.Events.Maintenance;

public record MaintenanceWorkOrderStartedEvent : IDomainEvent
{
    public Guid WorkOrderId { get; init; }
    public Guid AssetId { get; init; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public MaintenanceWorkOrderStartedEvent(Guid workOrderId, Guid assetId)
    {
        WorkOrderId = workOrderId;
        AssetId = assetId;
    }
}
