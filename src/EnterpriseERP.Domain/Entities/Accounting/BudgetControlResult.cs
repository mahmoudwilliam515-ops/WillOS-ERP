namespace EnterpriseERP.Domain.Entities.Accounting;

/// <summary>
/// Result of a budget control check.
/// RULE-BUDGET02: Always check before committing any expenditure.
/// </summary>
public class BudgetControlResult
{
    public bool IsAllowed { get; private set; }
    public BudgetControlViolationType ViolationType { get; private set; }
    public decimal RequestedAmount { get; private set; }
    public decimal AvailableBudget { get; private set; }
    public decimal EncumberedAmount { get; private set; }
    public decimal ActualSpent { get; private set; }
    public decimal OverrunAmount { get; private set; }
    public string Message { get; private set; } = string.Empty;

    public static BudgetControlResult Allowed(decimal requested, decimal available, decimal encumbered, decimal actual)
        => new()
        {
            IsAllowed        = true,
            ViolationType    = BudgetControlViolationType.None,
            RequestedAmount  = requested,
            AvailableBudget  = available,
            EncumberedAmount = encumbered,
            ActualSpent      = actual,
            OverrunAmount    = 0,
            Message          = "Budget check passed."
        };

    public static BudgetControlResult Blocked(BudgetControlViolationType violation, decimal requested, decimal available, decimal encumbered, decimal actual)
        => new()
        {
            IsAllowed        = false,
            ViolationType    = violation,
            RequestedAmount  = requested,
            AvailableBudget  = available,
            EncumberedAmount = encumbered,
            ActualSpent      = actual,
            OverrunAmount    = requested - (available - encumbered - actual),
            Message          = violation switch
            {
                BudgetControlViolationType.ExceedsBudget       => $"Amount {requested:N2} exceeds available budget {available:N2}. Overrun: {requested - (available - encumbered - actual):N2}",
                BudgetControlViolationType.NoBudgetFound       => "No approved budget found for this account and period.",
                BudgetControlViolationType.BudgetNotApproved   => "Budget exists but is not in Approved status.",
                BudgetControlViolationType.PeriodClosed        => "Accounting period is closed.",
                _                                              => "Budget control check failed."
            }
        };
}

public enum BudgetControlViolationType
{
    None               = 0,
    NoBudgetFound      = 1,
    BudgetNotApproved  = 2,
    ExceedsBudget      = 3,
    PeriodClosed       = 4
}
