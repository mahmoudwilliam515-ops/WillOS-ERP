using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Workflow;

public enum WorkflowDocumentType
{
    PurchaseOrder = 0,
    PurchaseInvoice = 1,
    SalesReturn = 2,
    PaymentVoucher = 3,
    JournalEntry = 4,
    Budget = 5,
    Generic = 99
}

public class WorkflowDefinition : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public WorkflowDocumentType DocumentType { get; set; }
    public decimal MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int RequiredApprovals { get; set; } = 1;
    public string ApproverRole { get; set; } = string.Empty;
    
    // Approval Matrix Dimensions
    public Guid? CompanyId { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? ProjectId { get; set; }

    // SLA & Escalation
    public int? SlaHours { get; set; }
    public string EscalationRole { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
