using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Treasury;

public class BankTransfer : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    
    // Can transfer from Bank to Bank, Bank to Cash, Cash to Bank, Cash to Cash
    public Guid? FromBankAccountId { get; set; }
    public Guid? FromCashAccountId { get; set; }
    
    public Guid? ToBankAccountId { get; set; }
    public Guid? ToCashAccountId { get; set; }
    
    public decimal Amount { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    
    public VoucherStatus Status { get; set; } = VoucherStatus.Draft;
    
    // GL Integration
    public Guid? JournalEntryId { get; set; }

    // Navigation
    public BankAccount? FromBankAccount { get; set; }
    public CashAccount? FromCashAccount { get; set; }
    public BankAccount? ToBankAccount { get; set; }
    public CashAccount? ToCashAccount { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
