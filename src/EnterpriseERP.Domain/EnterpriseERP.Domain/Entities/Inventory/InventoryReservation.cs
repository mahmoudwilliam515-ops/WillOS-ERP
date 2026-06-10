using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Inventory;

/// <summary>
/// Inventory Reservation: reserves stock for a specific order before it is dispatched.
/// This prevents overselling and ensures stock integrity.
/// </summary>
public class InventoryReservation : AuditableEntity
{
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>Reference to the document that triggered the reservation (e.g., SalesInvoice ID).</summary>
    public Guid ReferenceId { get; set; }
    public string ReferenceType { get; set; } = string.Empty; // "SalesInvoice", "ProductionOrder"
    public string ReferenceNumber { get; set; } = string.Empty;

    public decimal ReservedQuantity { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    public DateTime ExpiresAt { get; set; }
    public string? Notes { get; set; }
}

public enum ReservationStatus
{
    Active = 0,
    Fulfilled = 1,
    Cancelled = 2,
    Expired = 3
}
