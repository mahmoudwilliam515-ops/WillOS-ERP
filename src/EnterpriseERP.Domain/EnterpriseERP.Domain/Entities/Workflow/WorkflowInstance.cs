using EnterpriseERP.SharedKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseERP.Domain.Entities.Workflow;

public enum WorkflowInstanceStatus
{
    DRAFT = 0,
    PENDING_APPROVAL = 1,
    IN_REVIEW = 2,
    APPROVED = 3,
    REJECTED = 4,
    ESCALATED = 5,
    CANCELLED = 6,
    COMPLETED = 7
}

public enum WorkflowPriority
{
    LOW = 0,
    NORMAL = 1,
    HIGH = 2,
    CRITICAL = 3
}

public class WorkflowInstance : AuditableEntity, IAggregateRoot
{
    public Guid CompanyId { get; set; }
    public string WorkflowType { get; set; } = string.Empty; // PO_APPROVAL, INVOICE_APPROVAL, etc.
    public string ReferenceType { get; set; } = string.Empty; // PURCHASE_ORDER, AP_INVOICE, etc.
    public Guid ReferenceId { get; set; }
    public WorkflowInstanceStatus Status { get; set; } = WorkflowInstanceStatus.DRAFT;
    public string? TemporalRunId { get; set; }
    public short CurrentLevel { get; set; } = 1;
    public short TotalLevels { get; set; }
    public Guid RequesterId { get; set; }
    public decimal? RequestedAmount { get; set; }
    public string? CurrencyCode { get; set; }
    public DateTimeOffset SlaDeadline { get; set; }
    public WorkflowPriority Priority { get; set; } = WorkflowPriority.NORMAL;
    
    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}"; // Store JSON

    public DateTimeOffset? CompletedAt { get; set; }

    public ICollection<WorkflowTask> Tasks { get; set; } = new List<WorkflowTask>();
    public ICollection<WorkflowAuditLog> AuditLogs { get; set; } = new List<WorkflowAuditLog>();
}
