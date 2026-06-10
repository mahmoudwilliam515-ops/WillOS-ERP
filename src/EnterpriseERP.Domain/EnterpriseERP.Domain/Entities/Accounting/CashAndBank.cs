using EnterpriseERP.SharedKernel.Common;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Accounting;

public enum BankAccountType
{
    Checking = 0,
    Savings = 1,
    CreditCard = 2,
    Cash = 3
}

public class BankAccount : AuditableEntity, IAggregateRoot
{
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty; // IBAN
    public string BankName { get; set; } = string.Empty;
    public string Currency { get; set; } = "EGP";
    public BankAccountType Type { get; set; }
    
    public decimal CurrentBalance { get; set; }
    public decimal BankStatementBalance { get; set; } // الرصيد حسب آخر كشف حساب
    
    public Guid? LedgerAccountId { get; set; } // الربط بشجرة الحسابات
    public bool IsActive { get; set; } = true;
}

public class BankReconciliation : AuditableEntity, IAggregateRoot
{
    public Guid BankAccountId { get; set; }
    public DateTime StatementDate { get; set; }
    public decimal StatementEndingBalance { get; set; }
    
    public decimal TotalReconciled { get; set; }
    public bool IsCompleted { get; set; }
    
    public List<BankReconciliationLine> Lines { get; set; } = new();
}

public class BankReconciliationLine : BaseEntity
{
    public Guid BankReconciliationId { get; set; }
    public Guid JournalEntryLineId { get; set; } // الربط بحركة القيود
    public bool IsMatched { get; set; }
}
