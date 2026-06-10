using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Inventory;

public enum SerialStatus
{
    Available,
    Reserved,
    Sold,
    Returned,
    Lost
}

public class ItemSerial : AuditableEntity
{
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;
    
    public string SerialNumber { get; set; } = string.Empty;
    public SerialStatus Status { get; set; } = SerialStatus.Available;
    
    public Guid? CurrentWarehouseId { get; set; }
    public Warehouse? CurrentWarehouse { get; set; }
}
