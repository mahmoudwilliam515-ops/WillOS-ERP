using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Manufacturing;

public class RawMaterial : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // RULE-MFG01: Material Category
    public decimal StockLevel { get; set; }
    public decimal MinStockLevel { get; set; } // RULE-MFG02: Minimum Stock Level
    public decimal ReorderPoint { get; set; } // RULE-MFG03: Reorder Point
    public int LeadTimeDays { get; set; } = 0; // Days to procure
    public decimal CurrentCost { get; set; }
    public decimal CostPerUnit { get; set; }
    public Guid? SupplierId { get; set; } // RULE-MFG04: Primary Supplier

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
