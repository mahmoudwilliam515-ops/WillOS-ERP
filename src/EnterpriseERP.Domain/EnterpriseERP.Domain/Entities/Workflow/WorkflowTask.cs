using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Workflow;

public enum WorkflowTaskStatus
{
    PENDING = 0,
    VIEWED = 1,
    APPROVED = 2,
    REJECTED = 3,
    ESCALATED = 4,
    DELEGATED = 5,
    EXPIRED = 6
}

public class WorkflowTask : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Guid WorkflowInstanceId { get; set; }
    public WorkflowInstance WorkflowInstance { get; set; } = null!;

    public short LevelNumber { get; set; }
    public Guid AssigneeId { get; set; }
    public Guid? OriginalAssigneeId { get; set; }
    public Guid? DelegateId { get; set; }

    public WorkflowTaskStatus TaskStatus { get; set; } = WorkflowTaskStatus.PENDING;
    
    public DateTimeOffset AssignedAt { get; set; }
    public DateTimeOffset? ViewedAt { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
    public DateTimeOffset SlaDeadline { get; set; }

    public string? Decision { get; set; } // APPROVED / REJECTED
    public string? Comments { get; set; }
    
    public string? IpAddress { get; set; }
}
