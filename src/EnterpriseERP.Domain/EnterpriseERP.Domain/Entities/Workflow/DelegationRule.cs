using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Workflow;

public enum DelegationType
{
    FULL = 0,
    PARTIAL = 1
}

public class DelegationRule : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Guid DelegatorId { get; set; }
    public Guid DelegateId { get; set; }
    
    public DelegationType DelegationType { get; set; } = DelegationType.FULL;
    
    public string[]? WorkflowTypes { get; set; } // e.g. ["PO_APPROVAL", "INVOICE_APPROVAL"] for PARTIAL
    
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset ValidTo { get; set; }
    
    public string Reason { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? RevokedBy { get; set; }
}
