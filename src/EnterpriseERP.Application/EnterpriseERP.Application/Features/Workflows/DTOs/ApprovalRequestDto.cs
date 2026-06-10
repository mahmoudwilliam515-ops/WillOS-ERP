using EnterpriseERP.Domain.Entities.Workflow;

namespace EnterpriseERP.Application.Features.Workflows.DTOs;

public class ApprovalRequestDto
{
    public Guid Id { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public WorkflowDocumentType DocumentType { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public ApprovalRequestStatus Status { get; set; }
    public int RequiredApprovals { get; set; }
    public int ApprovalCount { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsEscalated { get; set; }
    public string? DelegatedToUserId { get; set; }
}
