using EnterpriseERP.SharedKernel.DomainEvents;

namespace EnterpriseERP.Application.Features.PurchaseInvoices.Events;

/// <summary>يُطلق بعد إنشاء فاتورة مشتريات جديدة بنجاح</summary>
public class PurchaseInvoiceCreatedEvent : BaseDomainEvent
{
    public Guid InvoiceId { get; }
    public Guid SupplierId { get; }
    public decimal TotalAmount { get; }

    public PurchaseInvoiceCreatedEvent(Guid invoiceId, Guid supplierId, decimal totalAmount)
    {
        InvoiceId = invoiceId;
        SupplierId = supplierId;
        TotalAmount = totalAmount;
    }
}

/// <summary>يُطلق بعد اعتماد فاتورة المشتريات وتسوية المخزون والمحاسبة</summary>
public class PurchaseInvoiceApprovedEvent : BaseDomainEvent
{
    public Guid InvoiceId { get; }
    public Guid SupplierId { get; }
    public decimal TotalAmount { get; }

    public PurchaseInvoiceApprovedEvent(Guid invoiceId, Guid supplierId, decimal totalAmount)
    {
        InvoiceId = invoiceId;
        SupplierId = supplierId;
        TotalAmount = totalAmount;
    }
}
