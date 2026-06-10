using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Sales;

/// <summary>
/// حجز المخزون — يُنشأ عند تأكيد Sales Order
/// يمنع بيع نفس الكمية لطلب آخر
/// Blueprint Section 1.2 — Order-to-Cash
/// </summary>
public class ReservationEntry : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public Guid SalesOrderLineId { get; private set; }
    public Guid ItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public DateTime ReservationDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public ReservationStatus Status { get; private set; }

    // EF Core constructor
    private ReservationEntry() { }

    public static ReservationEntry Create(
        Guid companyId,
        Guid salesOrderId,
        Guid salesOrderLineId,
        Guid itemId,
        Guid warehouseId,
        decimal reservedQuantity,
        DateTime reservationDate,
        DateTime? expiryDate = null)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (reservedQuantity <= 0)
            throw new ArgumentException("Reserved quantity must be positive.", nameof(reservedQuantity));

        return new ReservationEntry
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            SalesOrderId = salesOrderId,
            SalesOrderLineId = salesOrderLineId,
            ItemId = itemId,
            WarehouseId = warehouseId,
            ReservedQuantity = reservedQuantity,
            ReservationDate = reservationDate,
            ExpiryDate = expiryDate,
            Status = ReservationStatus.Active
        };
    }

    /// <summary>
    /// تحويل الحجز إلى إرسال فعلي (عند اكتمال Delivery Note)
    /// </summary>
    public void Fulfill()
    {
        if (Status != ReservationStatus.Active)
            throw new InvalidOperationException($"Cannot fulfill reservation in status {Status}.");
        Status = ReservationStatus.Fulfilled;
    }

    /// <summary>
    /// إلغاء الحجز (عند إلغاء Sales Order)
    /// </summary>
    public void Cancel()
    {
        if (Status == ReservationStatus.Fulfilled)
            throw new InvalidOperationException("Cannot cancel a fulfilled reservation.");
        Status = ReservationStatus.Cancelled;
    }
}

public enum ReservationStatus
{
    Active = 0,
    Fulfilled = 1,
    Cancelled = 2,
    Expired = 3
}
