using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Inventory;

public enum TransactionType
{
    SalesOut = 1,
    PurchaseIn = 2,
    AdjustmentIn = 3,
    AdjustmentOut = 4,
    TransferIn = 5,
    TransferOut = 6,
    ReturnFromCustomer = 7,
    ReturnToSupplier = 8,
    ManufacturingIn = 9,
    ManufacturingOut = 10,
    TransferTransit = 11
}

public class InventoryTransaction : AuditableEntity, IAggregateRoot
{
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? ReferenceId { get; set; }       // Invoice or document ID
    public string ReferenceType { get; set; } = string.Empty; // "SalesInvoice", "PurchaseInvoice", etc.
    public string ReferenceNumber { get; set; } = string.Empty;

    public TransactionType Type { get; set; }
    public decimal Quantity { get; set; }        // Positive = IN, Negative = OUT
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public Item Item { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
}

