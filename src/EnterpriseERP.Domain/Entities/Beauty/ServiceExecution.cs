using EnterpriseERP.Domain.Common;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Beauty;

/// <summary>
/// Records actual execution of a salon service.
/// Triggers material consumption from inventory based on ServiceRecipe.
/// KEY DIFFERENTIATOR: Links beauty service delivery to inventory deduction + profitability.
/// </summary>
public class ServiceExecution : AuditableEntity, ICompanyEntity
{
    public Guid CompanyId { get; set; }

    // Source appointment
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public Guid AppointmentLineId { get; set; }
    public AppointmentLine AppointmentLine { get; set; } = null!;

    // Service performed
    public Guid ServiceId { get; set; }
    public SalonService Service { get; set; } = null!;

    // Staff who performed the service
    public string StaffUserId { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;

    // Execution details
    public DateTime ExecutionStartTime { get; set; }
    public DateTime ExecutionEndTime { get; set; }
    public int ActualDurationMinutes => (int)(ExecutionEndTime - ExecutionStartTime).TotalMinutes;

    // Financials
    public decimal ServicePrice { get; set; }
    public decimal TotalMaterialCost { get; set; }
    public decimal GrossProfit => ServicePrice - TotalMaterialCost;
    public decimal GrossProfitPercent => ServicePrice > 0
        ? Math.Round(GrossProfit / ServicePrice * 100, 2) : 0;

    public ServiceExecutionStatus Status { get; set; } = ServiceExecutionStatus.Pending;

    public string? Notes { get; set; }

    // Material consumption lines
    public ICollection<ServiceExecutionMaterial> Materials { get; set; } = new List<ServiceExecutionMaterial>();
}

/// <summary>
/// Actual material consumed during service execution.
/// Created from Recipe but allows variance (e.g. stylist used more product).
/// </summary>
public class ServiceExecutionMaterial : AuditableEntity, ICompanyEntity
{
    public Guid CompanyId { get; set; }

    public Guid ServiceExecutionId { get; set; }
    public ServiceExecution ServiceExecution { get; set; } = null!;

    public Guid BeautyProductId { get; set; }
    public BeautyProduct BeautyProduct { get; set; } = null!;

    // From recipe (planned)
    public decimal PlannedQuantity { get; set; }
    public string Unit { get; set; } = "g";

    // Actual consumed
    public decimal ActualQuantity { get; set; }
    public decimal VarianceQuantity => ActualQuantity - PlannedQuantity;

    // Cost at time of execution (snapshot)
    public decimal UnitCost { get; set; }
    public decimal TotalCost => ActualQuantity * UnitCost;

    // Inventory transaction reference
    public Guid? InventoryTransactionId { get; set; }
}

public enum ServiceExecutionStatus
{
    Pending   = 0,
    InProgress = 1,
    Completed  = 2,
    Cancelled  = 3
}
