using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Manufacturing;

public class BillOfMaterialsLine : BaseEntity
{
    public Guid BillOfMaterialsId { get; set; }
    public BillOfMaterials BillOfMaterials { get; set; } = null!;

    public Guid RawMaterialId { get; set; }
    public RawMaterial RawMaterial { get; set; } = null!;
    
    public Guid? SubBOMId { get; set; } // For Multi-level BOM support
    public BillOfMaterials? SubBOM { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty; // RULE-MFG09: Unit of Measure
    public decimal ScrapFactor { get; set; } = 0; // RULE-MFG10: Scrap Factor (percentage)
}
