using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Accounting;

public enum AccountingPeriodStatus
{
    Open = 0,
    Closed = 1
}

public class AccountingPeriod : AuditableEntity
{
    public Guid CompanyId { get; private set; }
    public Guid FiscalYearId { get; set; }
    public FiscalYear FiscalYear { get; set; } = null!;
    
    public string PeriodName { get; set; } = string.Empty; // RULE-IFRS06: Period Name (Q1, Q2, Q3, Q4, Jan, Feb, etc.)
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AccountingPeriodStatus Status { get; set; } = AccountingPeriodStatus.Open; // RULE-IFRS07: Status (Open/Closed)

    // Period Closing Checklist (IFRS Compliance)
    public bool IsInventoryReconciled { get; set; }
    public bool IsARReconciled { get; set; }
    public bool IsAPReconciled { get; set; }
    public bool IsFixedAssetsDepreciated { get; set; }
    public bool IsPayrollPosted { get; set; }
    public bool IsBankReconciled { get; set; }

    public string? ClosedBy { get; set; }
    public DateTime? ClosedAt { get; set; }
}
