using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Accounting;

/// <summary>
/// A single line in a budget, linking an account to a monthly planned amount.
/// </summary>
public class BudgetLine : AuditableEntity
{
    public Guid BudgetId { get; set; }
    public Budget Budget { get; set; } = null!;
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    // Monthly planned amounts
    public decimal Jan { get; set; }
    public decimal Feb { get; set; }
    public decimal Mar { get; set; }
    public decimal Apr { get; set; }
    public decimal May { get; set; }
    public decimal Jun { get; set; }
    public decimal Jul { get; set; }
    public decimal Aug { get; set; }
    public decimal Sep { get; set; }
    public decimal Oct { get; set; }
    public decimal Nov { get; set; }
    public decimal Dec { get; set; }

    public decimal TotalPlanned =>
        Jan + Feb + Mar + Apr + May + Jun +
        Jul + Aug + Sep + Oct + Nov + Dec;

    public string? Notes { get; set; }
}
