using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Purchasing;

public enum PurchaseOrderStatus
{
    Draft    = 0,
    Approved = 1,
    Sent     = 2,
    Received = 3,
    Closed   = 4,
    Cancelled= 5,
    PartiallyReceived = 6
}

public class PurchaseOrder : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public Guid SupplierId { get; set; }
    public Guid BranchId { get; set; }
    public Guid WarehouseId { get; set; }

    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string Notes { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    // Navigation
    public Supplier Supplier { get; set; } = null!;
    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public class PurchaseOrderLine : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }
    public Guid ItemId { get; set; }

    public Guid? ProjectTaskId { get; set; } // Link to Project Task for commitments

    public decimal Quantity { get; set; }
    public decimal OrderedQuantity => Quantity;   // alias
    public decimal UnitCost { get; set; }
    public decimal UnitPrice => UnitCost;          // alias
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    public string Notes { get; set; } = string.Empty;

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
