using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Inventory;

public enum AdjustmentType
{
    Increase,
    Decrease,
    Damage,
    Theft,
    Found
}

public class InventoryAdjustment : AuditableEntity, IAggregateRoot
{
    public string AdjustmentNumber { get; set; } = string.Empty;
    
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;
    
    public decimal Quantity { get; set; }
    public AdjustmentType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    
    public Guid? RelatedJournalEntryId { get; set; }
}
