using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Workflow;

public enum ApprovalActionType
{
    Approved = 0,
    Rejected = 1
}

public class ApprovalAction : AuditableEntity
{
    public Guid ApprovalRequestId { get; set; }
    public ApprovalRequest ApprovalRequest { get; set; } = null!;
    public ApprovalActionType ActionType { get; set; }
    public string ActorUserId { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime ActionAt { get; set; } = DateTime.UtcNow;
}
