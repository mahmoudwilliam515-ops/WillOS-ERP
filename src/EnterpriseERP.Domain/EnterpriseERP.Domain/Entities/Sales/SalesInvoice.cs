using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.SharedKernel.Exceptions;
using EnterpriseERP.Domain.Common;
using EnterpriseERP.Domain.Enums;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseERP.Domain.Entities.Sales;

public enum InvoiceStatus
{
    Draft = 0,
    Approved = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Cancelled = 4,
    Overdue = 5
}

public class SalesInvoice : AuditableEntity, IAggregateRoot, ISoftDelete, ICompanyEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; private set; }
    public Guid CustomerId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? DeliveryNoteId { get; set; } // For Order-to-Cash fulfillment gate
    
    public decimal SubTotal { get; set; }      // Before discount
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }   // Final amount
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }

    /// <summary>حالة التسوية</summary>
    public InvoiceSettlementStatus SettlementStatus { get; private set; }
        = InvoiceSettlementStatus.Outstanding;

    public string Notes { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    // E-Invoice compliance (ZATCA / ETA)
    public EInvoiceAuthority EInvoiceAuthority { get; set; } = EInvoiceAuthority.None;
    public EInvoiceSubmissionStatus EInvoiceStatus { get; set; } = EInvoiceSubmissionStatus.NotSubmitted;
    public string? EInvoiceUuid { get; set; }
    public string? EInvoiceQrPayload { get; set; }
    public string? EInvoiceXmlHash { get; set; }
    public string? EInvoiceResponseMessage { get; set; }
    public DateTime? EInvoiceSubmittedAt { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public ICollection<SalesInvoiceLine> Lines { get; set; } = new List<SalesInvoiceLine>();
    public ICollection<InvoicePayment> Payments { get; private set; } = new List<InvoicePayment>();

    public static SalesInvoice Create(Guid customerId, Guid companyId, Guid tenantId, DateTime date, decimal totalAmount, string? invoiceNumber = null)
    {
        return new SalesInvoice
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CompanyId = companyId,
            TenantId = tenantId,
            InvoiceDate = date,
            TotalAmount = totalAmount,
            InvoiceNumber = invoiceNumber ?? $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Status = InvoiceStatus.Draft
        };
    }

    // ── Settlement Logic ──────────────────────────────────────

    /// <summary>
    /// تُطبَّق عند إنشاء الفاتورة — تضبط RemainingAmount = TotalAmount
    /// </summary>
    public void InitializeSettlement()
    {
        if (RemainingAmount != 0)
            throw new SalesDomainException(
                $"لا يمكن تهيئة التسوية مرتين. الفاتورة: {Id}");

        RemainingAmount = TotalAmount;
        SettlementStatus = InvoiceSettlementStatus.Outstanding;
    }

    /// <summary>
    /// تُطبَّق عند تسجيل دفعة — تُخفِّض RemainingAmount
    /// وتُحدِّث SettlementStatus تلقائياً
    /// </summary>
    /// <param name="paymentAmount">مبلغ الدفعة الجديدة</param>
    public void ApplyPayment(decimal paymentAmount)
    {
        if (paymentAmount <= 0)
            throw new SalesDomainException(
                $"مبلغ الدفعة يجب أن يكون موجباً. القيمة: {paymentAmount}");

        if (paymentAmount > RemainingAmount)
            throw new SalesDomainException(
                $"مبلغ الدفعة ({paymentAmount:N2}) يتجاوز الرصيد المتبقي " +
                $"({RemainingAmount:N2}) للفاتورة {InvoiceNumber}. " +
                $"استخدم Credit Note لمعالجة الدفع الزائد.");

        if (SettlementStatus == InvoiceSettlementStatus.FullyPaid)
            throw new SalesDomainException(
                $"الفاتورة {InvoiceNumber} مُسدَّدة بالكامل مسبقاً.");

        PaidAmount += paymentAmount;
        RemainingAmount -= paymentAmount;

        SettlementStatus = RemainingAmount == 0
            ? InvoiceSettlementStatus.FullyPaid
            : InvoiceSettlementStatus.PartiallyPaid;
    }

    /// <summary>
    /// يُستخدم عند عكس دفعة (مثلاً عند إلغاء Receipt Voucher)
    /// </summary>
    public void ReversePayment(decimal reversalAmount)
    {
        if (reversalAmount <= 0)
            throw new SalesDomainException(
                $"مبلغ العكس يجب أن يكون موجباً. القيمة: {reversalAmount}");

        PaidAmount -= reversalAmount;
        RemainingAmount += reversalAmount;

        if (RemainingAmount > TotalAmount)
            throw new SalesDomainException(
                $"RemainingAmount ({RemainingAmount:N2}) لا يمكن أن يتجاوز " +
                $"TotalAmount ({TotalAmount:N2}) للفاتورة {InvoiceNumber}.");

        SettlementStatus = RemainingAmount == TotalAmount
            ? InvoiceSettlementStatus.Outstanding
            : InvoiceSettlementStatus.PartiallyPaid;
    }

    public void ApplyCreditNote(Guid creditNoteId, decimal amount)
    {
        if (amount <= 0) throw new SalesDomainException("Amount must be positive");
        RemainingAmount -= amount;
        if (RemainingAmount < 0) RemainingAmount = 0;
        
        SettlementStatus = RemainingAmount == 0
            ? InvoiceSettlementStatus.FullyPaid
            : InvoiceSettlementStatus.PartiallyPaid;
    }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public void Approve(string approvedBy, DateTime approvedAt)
    {
        if (Status != InvoiceStatus.Draft)
            throw new SalesDomainException($"Cannot approve invoice in {Status} status.");

        Status = InvoiceStatus.Approved;
        // You might want to store who approved it and when
    }
}

