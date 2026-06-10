using EnterpriseERP.SharedKernel.Common;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Inventory;

public enum TransferStatus
{
    Draft = 0,
    Shipped = 1,
    InTransit = 2,
    Received = 3,
    Cancelled = 4
}

public class TransferOrder : AuditableEntity, IAggregateRoot
{
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    
    public Guid FromWarehouseId { get; set; }
    public Warehouse FromWarehouse { get; set; } = null!;
    
    public Guid ToWarehouseId { get; set; }
    public Warehouse ToWarehouse { get; set; } = null!;
    
    public TransferStatus Status { get; set; } = TransferStatus.Draft;
    public string Notes { get; set; } = string.Empty;
    
    public List<TransferOrderLine> Lines { get; set; } = new();
}

public class TransferOrderLine : BaseEntity
{
    public Guid TransferOrderId { get; set; }
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    
    public Guid? FromBinId { get; set; }
    public Guid? ToBinId { get; set; }
}
