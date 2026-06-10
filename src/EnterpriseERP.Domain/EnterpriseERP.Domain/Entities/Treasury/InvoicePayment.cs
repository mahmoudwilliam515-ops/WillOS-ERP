// ============================================================
// P0-Task-3: AR Settlement — كيان InvoicePayment كامل
// الملف: src/Domain/Entities/Treasury/InvoicePayment.cs
// المرجع: Blueprint Section 1.2 (O2C) + ERP_Required_Skills.md §5
// ============================================================
// القاعدة: لا Fallback صامت — كل Exception يجب أن يكون صريحاً
// القاعدة: TenantId في كل Entity
// القاعدة: AuditableEntity دائماً
// ============================================================

using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Entities.Sales;

namespace EnterpriseERP.Domain.Entities.Treasury;

/// <summary>
/// يُمثّل تسوية دفعة مقابل فاتورة مبيعات محددة.
/// كل Receipt Voucher يمكن أن يُسوَّى مقابل فواتير متعددة.
/// القاعدة: مجموع AllocatedAmount لكل فاتورة يجب أن = RemainingAmount المُخفَّض
/// </summary>
public class InvoicePayment : AuditableEntity
{
    // ── المعرّفات ─────────────────────────────────────────────

    /// <summary>الفاتورة التي يُسوَّى معها (AR)</summary>
    public Guid? SalesInvoiceId { get; set; }

    /// <summary>فاتورة المشتريات التي يُسوَّى معها (AP)</summary>
    public Guid? PurchaseInvoiceId { get; set; }

    /// <summary>إيصال القبض (AR) أو سند الصرف (AP)</summary>
    public Guid? ReceiptVoucherId { get; set; }
    public Guid? PaymentVoucherId { get; set; }

    // ── المبالغ ───────────────────────────────────────────────

    /// <summary>المبلغ المُخصَّص</summary>
    public decimal AllocatedAmount { get; set; }

    // For backward compatibility with existing code
    public decimal Amount
    {
        get => AllocatedAmount;
        set => AllocatedAmount = value;
    }

    /// <summary>تاريخ التسوية الفعلية</summary>
    public DateTime SettlementDate { get; set; }

    /// <summary>ملاحظات التسوية (اختياري)</summary>
    public string? Notes { get; set; }  // public set — يسمح للـ initializer وللـ EF Core بالضبط

    // ── Navigation Properties ─────────────────────────────────

    public SalesInvoice? SalesInvoice { get; set; }
    public ReceiptVoucher? ReceiptVoucher { get; set; }

    // ── Constructor ───────────────────────────────────────────

    public InvoicePayment() { } // Public for EF Core + existing commands

    public static InvoicePayment Create(
        Guid salesInvoiceId,
        Guid receiptVoucherId,
        decimal allocatedAmount,
        DateTime settlementDate,
        Guid tenantId,
        string? notes = null)
    {
        if (allocatedAmount <= 0)
            throw new InvalidOperationException(
                $"AllocatedAmount يجب أن يكون موجباً. القيمة: {allocatedAmount}");

        return new InvoicePayment
        {
            SalesInvoiceId = salesInvoiceId,
            ReceiptVoucherId = receiptVoucherId,
            AllocatedAmount = allocatedAmount,
            SettlementDate = settlementDate.Date,
            Notes = notes,
            TenantId = tenantId
        };
    }
}
