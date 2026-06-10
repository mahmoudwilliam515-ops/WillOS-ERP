using EnterpriseERP.SharedKernel.DomainEvents;

namespace EnterpriseERP.Application.Features.SalesInvoices.Events;

/// <summary>يُطلق بعد إنشاء فاتورة بيع جديدة بنجاح</summary>
public class SalesInvoiceCreatedEvent : BaseDomainEvent
{
    public Guid InvoiceId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }
    public string InvoiceNumber { get; }

    public SalesInvoiceCreatedEvent(Guid invoiceId, Guid customerId, decimal totalAmount, string invoiceNumber)
    {
        InvoiceId = invoiceId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        InvoiceNumber = invoiceNumber;
    }
}

/// <summary>يُطلق بعد اعتماد فاتورة البيع وتسوية المخزون والمحاسبة</summary>
public class SalesInvoiceApprovedEvent : BaseDomainEvent
{
    public Guid InvoiceId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }

    public SalesInvoiceApprovedEvent(Guid invoiceId, Guid customerId, decimal totalAmount)
    {
        InvoiceId = invoiceId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}
