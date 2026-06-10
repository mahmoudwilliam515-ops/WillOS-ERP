using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Manufacturing;

public class ProductionOrderMaterial : BaseEntity
{
    public Guid ProductionOrderId { get; set; }
    public ProductionOrder ProductionOrder { get; set; } = null!;

    public Guid RawMaterialId { get; set; } // RULE-MFG21: Raw Material Reference
    public RawMaterial RawMaterial { get; set; } = null!;

    public decimal PlannedQuantity { get; set; } // RULE-MFG22: Planned Quantity
    public decimal ActualQuantity { get; set; } // RULE-MFG23: Actual Quantity
    public decimal ScrapQuantity { get; set; } // RULE-MFG24: Scrap Quantity
    
    public decimal UnitCost { get; set; }
    public decimal TotalCost => ActualQuantity * UnitCost;
}
