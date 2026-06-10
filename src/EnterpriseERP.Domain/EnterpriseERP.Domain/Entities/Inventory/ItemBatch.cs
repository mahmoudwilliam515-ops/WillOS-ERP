using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Inventory;

public class ItemBatch : AuditableEntity
{
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;
    
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    public decimal InitialQuantity { get; set; }
    public decimal CurrentQuantity { get; set; }
    public bool IsActive { get; set; } = true;
}
