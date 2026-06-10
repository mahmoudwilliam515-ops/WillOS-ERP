using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Inventory;

public class WarehouseBin : AuditableEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    
    public string Code { get; set; } = string.Empty; // e.g., A1-S2-B3 (Aisle 1, Shelf 2, Bin 3)
    public string Zone { get; set; } = string.Empty; // e.g., Cold Storage, Dry Goods
    
    public bool IsActive { get; set; } = true;
    public decimal MaxCapacity { get; set; }
    public decimal CurrentVolume { get; set; }
}
