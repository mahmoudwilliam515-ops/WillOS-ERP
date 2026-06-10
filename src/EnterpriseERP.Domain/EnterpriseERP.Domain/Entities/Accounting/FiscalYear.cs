using EnterpriseERP.SharedKernel.Common;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Accounting;

public enum FiscalYearStatus
{
    Open = 0,
    Closed = 1
}

public class FiscalYear : AuditableEntity, IAggregateRoot
{
    public int Year { get; set; } // RULE-IFRS03: Fiscal Year (e.g., 2024)
    public string Name { get; set; } = string.Empty; // Display name (e.g., "FY 2024")
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public FiscalYearStatus Status { get; set; } = FiscalYearStatus.Open; // RULE-IFRS04: Status (Open/Closed)
    public bool IsCurrent { get; set; } = false; // RULE-IFRS05: Current fiscal year indicator
    
    public ICollection<AccountingPeriod> Periods { get; set; } = new List<AccountingPeriod>();
}
