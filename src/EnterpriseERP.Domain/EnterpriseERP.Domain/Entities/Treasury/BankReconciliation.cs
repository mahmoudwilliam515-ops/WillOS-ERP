using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Treasury;

public class BankReconciliation : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    
    public DateTime StatementDate { get; set; }
    public decimal StatementEndingBalance { get; set; }
    public decimal ClearedBalance { get; set; }
    public decimal Difference => StatementEndingBalance - ClearedBalance;
    
    public bool IsClosed { get; set; }

    public ICollection<BankReconciliationLine> Lines { get; set; } = new List<BankReconciliationLine>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public class BankReconciliationLine : AuditableEntity
{
    public Guid BankReconciliationId { get; set; }
    public BankReconciliation BankReconciliation { get; set; } = null!;
    
    public Guid TransactionId { get; set; } // Can be ReceiptVoucher, PaymentVoucher, etc.
    public decimal Amount { get; set; }
    public bool IsCleared { get; set; }
}
