using EnterpriseERP.SharedKernel.Common;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Accounting;

public class CostCenter : AuditableEntity, IAggregateRoot
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    public Guid? ParentId { get; set; }
    public CostCenter? Parent { get; set; }
    
    public ICollection<CostCenter> SubCostCenters { get; set; } = new List<CostCenter>();
}
