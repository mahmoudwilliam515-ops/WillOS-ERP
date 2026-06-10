using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Sales;

public enum QuotationStatus
{
    Draft     = 0,
    Sent      = 1,
    Approved  = 2,
    Rejected  = 3,
    Expired   = 4,
    Converted = 5
}

public class SalesQuotation : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime QuotationDate { get; set; }
    public DateTime ValidUntil { get; set; }
    public Guid CustomerId { get; set; }
    public Guid BranchId { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string Notes { get; set; } = string.Empty;
    public QuotationStatus Status { get; set; } = QuotationStatus.Draft;

    // Navigation
    public Customer Customer { get; set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
