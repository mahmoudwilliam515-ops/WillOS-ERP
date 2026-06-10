using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Manufacturing;

public class ProductionStage : AuditableEntity
{
    public Guid BillOfMaterialsId { get; set; }
    public virtual BillOfMaterials BillOfMaterials { get; set; } = null!;

    public string Name { get; set; } = string.Empty; // RULE-MFG11: Stage Name
    public int Sequence { get; set; } // RULE-MFG12: Sequence Order
    public decimal EstimatedHours { get; set; } // RULE-MFG13: Estimated Hours
    public decimal CostPerHour { get; set; }
    public Guid? WorkCenterId { get; set; }
    public WorkCenter? WorkCenter { get; set; }
    public string Description { get; set; } = string.Empty;
}
