using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Manufacturing;

public class ProductionOrderStage : AuditableEntity
{
    public Guid ProductionOrderId { get; set; }
    public virtual ProductionOrder ProductionOrder { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public int Sequence { get; set; } // RULE-MFG25: Sequence Order
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal CostPerHour { get; set; }
    public Guid? WorkCenterId { get; set; }
    public WorkCenter? WorkCenter { get; set; }
    
    public ProductionStageStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public string? MachineId { get; set; }
    public string? OperatorId { get; set; }
    public string? Notes { get; set; }
}

public enum ProductionStageStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Skipped = 3
}
