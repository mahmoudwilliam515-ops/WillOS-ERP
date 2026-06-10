using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Treasury;

/// <summary>
/// سند الصرف — دفعة خارجة (للمورد أو تحويل بنكي)
/// يستخدم VoucherStatus المُعرَّف في VoucherStatus.cs
/// PaymentVoucherStatus مُعرَّف هنا للتوافق مع الكود الذي يحتاجه صراحةً
/// </summary>
public class PaymentVoucher : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string VoucherNumber { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? BankStatementReference { get; set; }
    public string? Reference { get; set; }

    public Guid? SupplierId { get; set; } // If paying to Supplier
    public Guid CompanyId { get; set; }
    public Guid? PurchaseInvoiceId { get; set; } // AP Settlement

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
    // Supplier navigation — Guid FK only (avoid circular dependency with Purchasing)
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

/// <summary>
/// حالة سند الصرف بتفاصيل أكثر من VoucherStatus — مطلوب لـ TreasuryCommandHandlers
/// </summary>
public enum PaymentVoucherStatus
{
    Draft = 0,
    Approved = 1,
    Posted = 2,
    Cancelled = 3
}
