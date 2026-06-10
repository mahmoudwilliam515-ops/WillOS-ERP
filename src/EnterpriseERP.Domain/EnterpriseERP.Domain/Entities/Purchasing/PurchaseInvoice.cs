using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseERP.Domain.Entities.Purchasing;

public enum PurchaseInvoiceStatus
{
    Draft = 0,
    Approved = 1,
    Cancelled = 2,
    Disputed = 3,
    Matched = 4,
    OnHold = 5,
    ApprovedForPayment = 6
}

public class PurchaseInvoice : AuditableEntity, IAggregateRoot, ISoftDelete, ICompanyEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public Guid SupplierId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? PurchaseOrderId { get; set; } // For 3-way matching
    public Guid BranchId { get; set; }
    public Guid WarehouseId { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }

    public string Notes { get; set; } = string.Empty;
    public PurchaseInvoiceStatus Status { get; set; } = PurchaseInvoiceStatus.Draft;
    public InvoiceMatchStatus MatchStatus { get; set; } = InvoiceMatchStatus.Unmatched;

    public void ApplyPayment(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive");
        PaidAmount += amount;
        RemainingAmount = TotalAmount - PaidAmount;
        if (RemainingAmount <= 0)
        {
            Status = PurchaseInvoiceStatus.ApprovedForPayment; // Or whatever is appropriate, perhaps Paid if it existed
        }
    }

    public void SetMatchStatus(InvoiceMatchStatus status)
    {
        MatchStatus = status;
    }

    // Navigation
    public Supplier Supplier { get; set; } = null!;
    public ICollection<PurchaseInvoiceLine> Lines { get; set; } = new List<PurchaseInvoiceLine>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public enum InvoiceMatchStatus
{
    Unmatched = 0,
    PartiallyMatched = 1,
    Matched = 2,
    Exception = 3
}

