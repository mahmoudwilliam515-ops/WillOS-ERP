using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Treasury;

public class ReceiptVoucher : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string VoucherNumber { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public DateTime ReceiptDate { get; set; }
    public string? BankStatementReference { get; set; }
    public string? Reference { get; set; }
    
    public Guid? CustomerId { get; set; } // If receiving from Customer
    public Guid CompanyId { get; set; }
    public Guid? SalesInvoiceId { get; set; } // AR Settlement
    
    
    // Treasury Source
    public Guid? CashAccountId { get; set; }
    public Guid? BankAccountId { get; set; }
    
    public decimal Amount { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty; // e.g. Check number
    public string Notes { get; set; } = string.Empty;
    
    public VoucherStatus Status { get; set; } = VoucherStatus.Draft;
    
    // GL Integration
    public Guid? JournalEntryId { get; set; }
    
    // Navigation
    public CashAccount? CashAccount { get; set; }
    public BankAccount? BankAccount { get; set; }
    public ICollection<InvoicePayment> InvoicePayments { get; set; } = new List<InvoicePayment>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public void SetBankStatementReference(string? reference)
    {
        BankStatementReference = reference;
    }
}
