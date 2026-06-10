using EnterpriseERP.Application.Features.Workflows.DTOs;
using EnterpriseERP.Domain.Entities.Workflow;
using MediatR;

namespace EnterpriseERP.Application.Features.Workflows.Commands.SubmitApprovalRequest;

public class SubmitApprovalRequestCommand : IRequest<ApprovalRequestDto>
{
    public WorkflowDocumentType DocumentType { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
