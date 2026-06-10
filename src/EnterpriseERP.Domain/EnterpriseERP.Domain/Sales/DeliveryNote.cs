using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Sales.Events;

namespace EnterpriseERP.Domain.Sales;

/// <summary>
/// إشعار التسليم — الحلقة الثالثة في O2C (بعد تأكيد أمر البيع)
/// Fulfillment Gate: لا فاتورة بيع بدون إشعار تسليم مكتمل
/// </summary>
public class DeliveryNote : AuditableEntity
{
    public Guid Id { get; private set; }
    public string DeliveryNumber { get; private set; } = default!;
    public Guid CompanyId { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public DateTime DeliveryDate { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public string? TrackingNumber { get; private set; }
    public string? Notes { get; private set; }
    public Guid? SalesInvoiceId { get; private set; }

    private readonly List<DeliveryNoteLine> _lines = new();
    public IReadOnlyCollection<DeliveryNoteLine> Lines => _lines.AsReadOnly();

    private DeliveryNote() { }

    public static DeliveryNote Create(
        Guid companyId,
        Guid salesOrderId,
        Guid customerId,
        Guid warehouseId,
        DateTime deliveryDate,
        string deliveryNumber,
        string? trackingNumber = null,
        string? notes = null)
    {
        return new DeliveryNote
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            SalesOrderId = salesOrderId,
            CustomerId = customerId,
            WarehouseId = warehouseId,
            DeliveryDate = deliveryDate,
            DeliveryNumber = deliveryNumber,
            TrackingNumber = trackingNumber,
            Notes = notes,
            Status = DeliveryStatus.Draft
        };
    }

    public void AddLine(Guid salesOrderLineId, Guid itemId, decimal deliveredQuantity)
    {
        if (Status != DeliveryStatus.Draft)
            throw new InvalidOperationException($"Cannot modify Delivery Note in status {Status}");

        _lines.Add(DeliveryNoteLine.Create(Id, salesOrderLineId, itemId, deliveredQuantity));
    }

    /// <summary>
    /// تأكيد التسليم — يُغيِّر حالة أمر البيع إلى Shipped
    /// يُفعِّل Fulfillment Gate للفوترة
    /// </summary>
    public void Complete()
    {
        if (Status != DeliveryStatus.Draft)
            throw new InvalidOperationException($"Cannot complete Delivery Note in status {Status}");

        if (!_lines.Any())
            throw new InvalidOperationException("Cannot complete Delivery Note with no lines");

        Status = DeliveryStatus.Completed;
        AddDomainEvent(new DeliveryNoteCompletedEvent(Id, CompanyId, SalesOrderId, _lines.ToList()));
    }
}

public class DeliveryNoteLine : BaseEntity
{
    public Guid Id { get; private set; }
    public Guid DeliveryNoteId { get; private set; }
    public Guid SalesOrderLineId { get; private set; }
    public Guid ItemId { get; private set; }
    public decimal DeliveredQuantity { get; private set; }

    private DeliveryNoteLine() { }

    internal static DeliveryNoteLine Create(
        Guid deliveryNoteId,
        Guid salesOrderLineId,
        Guid itemId,
        decimal deliveredQuantity)
    {
        if (deliveredQuantity <= 0) throw new ArgumentException("Delivered quantity must be positive");

        return new DeliveryNoteLine
        {
            Id = Guid.NewGuid(),
            DeliveryNoteId = deliveryNoteId,
            SalesOrderLineId = salesOrderLineId,
            ItemId = itemId,
            DeliveredQuantity = deliveredQuantity
        };
    }
}

public enum DeliveryStatus { Draft, Completed, Cancelled }
