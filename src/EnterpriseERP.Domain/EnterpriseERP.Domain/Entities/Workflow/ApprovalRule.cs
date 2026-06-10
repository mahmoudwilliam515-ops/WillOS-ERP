using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Workflow;

public class ApprovalRule : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public string WorkflowType { get; set; } = string.Empty;
    public short LevelNumber { get; set; }
    public string ApproverType { get; set; } = string.Empty; // ROLE, USER, DYNAMIC_MANAGER, etc.
    public string ApproverRef { get; set; } = string.Empty; 
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? DepartmentScope { get; set; }
    public string? CostCenterScope { get; set; }
    public bool RequiresComment { get; set; }
    public bool ParallelApproval { get; set; }
    public short SlaHours { get; set; }
    public Guid? EscalationRuleId { get; set; } // Can map to an EscalationRule entity if needed
    public bool IsActive { get; set; } = true;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}
