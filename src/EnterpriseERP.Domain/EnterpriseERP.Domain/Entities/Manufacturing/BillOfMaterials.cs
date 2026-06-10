using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Common;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Manufacturing;

public enum BOMStatus
{
    Active = 0,
    Obsolete = 1
}

public class BillOfMaterials : AuditableEntity, IAggregateRoot, ICompanyEntity
{
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public Item Product { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0"; // RULE-MFG05: BOM Version
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow; // RULE-MFG06: Effective Date
    public BOMStatus Status { get; set; } = BOMStatus.Active; // RULE-MFG07: Status (Active/Obsolete)
    public bool IsDefault { get; set; }
    
    // Costing
    public decimal LaborCostPerHour { get; set; }
    public decimal MachineOverheadPerHour { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    
    public virtual ICollection<BillOfMaterialsLine> Lines { get; set; } = new List<BillOfMaterialsLine>();
    public virtual ICollection<ProductionStage> Stages { get; set; } = new List<ProductionStage>();
}
