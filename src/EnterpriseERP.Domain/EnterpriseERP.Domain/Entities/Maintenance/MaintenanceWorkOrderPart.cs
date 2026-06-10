using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Entities.Inventory;
using System;

namespace EnterpriseERP.Domain.Entities.Maintenance;

public class MaintenanceWorkOrderPart : BaseEntity
{
    public Guid WorkOrderId { get; set; }
    public MaintenanceWorkOrder WorkOrder { get; set; } = null!;
    
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;
    
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost => Quantity * UnitCost;
}
