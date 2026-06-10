using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Inventory;

public enum InventoryTransferStatus
{
    Draft,
    InTransit,
    Completed,
    Cancelled
}

public class InventoryTransfer : AuditableEntity, IAggregateRoot
{
    public string TransferNumber { get; set; } = string.Empty;
    
    public Guid FromWarehouseId { get; set; }
    public Warehouse FromWarehouse { get; set; } = null!;
    
    public Guid ToWarehouseId { get; set; }
    public Warehouse ToWarehouse { get; set; } = null!;
    
    public DateTime TransferDate { get; set; }
    public InventoryTransferStatus Status { get; set; } = InventoryTransferStatus.Draft;
    
    public string Remarks { get; set; } = string.Empty;
}
