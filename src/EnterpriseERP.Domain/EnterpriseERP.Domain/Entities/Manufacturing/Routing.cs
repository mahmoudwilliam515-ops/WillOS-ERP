using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Manufacturing;

public enum WorkCenterStatus
{
    Available = 0,
    UnderMaintenance = 1,
    OutOfOrder = 2,
    Occupied = 3
}

public class WorkCenter : AuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public decimal HourlyRate { get; set; } // Labor cost per hour
    public decimal OverheadRate { get; set; } // Factory overhead per hour
    public decimal EfficiencyFactor { get; set; } = 1.0m; // e.g., 0.8 for 80% efficiency
    
    public bool IsActive { get; set; } = true;
    public WorkCenterStatus Status { get; set; } = WorkCenterStatus.Available;
}

public class Routing : AuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ProductId { get; set; } // Linked to an item
    
    public List<RoutingStep> Steps { get; set; } = new();
}

public class RoutingStep : BaseEntity
{
    public int Sequence { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public Guid WorkCenterId { get; set; }
    public WorkCenter WorkCenter { get; set; } = null!;
    
    public decimal SetupTimeMinutes { get; set; }
    public decimal RunTimeMinutesPerUnit { get; set; }
    
    public string Instructions { get; set; } = string.Empty;
}
