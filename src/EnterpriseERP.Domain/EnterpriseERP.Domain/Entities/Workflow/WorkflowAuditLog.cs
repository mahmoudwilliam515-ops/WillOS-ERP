using EnterpriseERP.SharedKernel.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseERP.Domain.Entities.Workflow;

public class WorkflowAuditLog : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Guid WorkflowInstanceId { get; set; }
    public WorkflowInstance WorkflowInstance { get; set; } = null!;

    public Guid? TaskId { get; set; }
    public WorkflowTask? Task { get; set; }

    public string EventType { get; set; } = string.Empty; 
    // WORKFLOW_STARTED / TASK_ASSIGNED / TASK_APPROVED / TASK_REJECTED / ESCALATED / DELEGATED / CANCELLED / COMPLETED
    
    public Guid? ActorId { get; set; }
    public string? ActorRole { get; set; }
    public string? ActorIp { get; set; }
    public string? ActorDevice { get; set; }

    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string EventPayload { get; set; } = "{}";

    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
