using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Workflow;

public enum ApprovalRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3,
    Escalated = 4
}

public class ApprovalRequest : AuditableEntity, IAggregateRoot
{
    public Guid WorkflowDefinitionId { get; set; }
    public WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public WorkflowDocumentType DocumentType { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public ApprovalRequestStatus Status { get; set; } = ApprovalRequestStatus.Pending;
    public int RequiredApprovals { get; set; } = 1;
    public int ApprovalCount { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // SLA & Escalation
    public DateTime? DueDate { get; set; }
    public bool IsEscalated { get; set; } = false;

    // Delegation
    public string? DelegatedToUserId { get; set; }

    public string Notes { get; set; } = string.Empty;

    public ICollection<ApprovalAction> Actions { get; set; } = new List<ApprovalAction>();
}
