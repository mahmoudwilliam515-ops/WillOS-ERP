using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Accounting;

/// <summary>
/// Represents a budget for a specific fiscal year and cost center.
/// Tracks planned vs actual expenditure per account.
/// </summary>
public class Budget : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid FiscalYearId { get; set; }
    public FiscalYear FiscalYear { get; set; } = null!;
    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
    public BudgetStatus Status { get; set; } = BudgetStatus.Draft;
    public string? Notes { get; set; }

    public ICollection<BudgetLine> Lines { get; set; } = new List<BudgetLine>();
}

public enum BudgetStatus
{
    Draft = 0,
    Approved = 1,
    Closed = 2
}
