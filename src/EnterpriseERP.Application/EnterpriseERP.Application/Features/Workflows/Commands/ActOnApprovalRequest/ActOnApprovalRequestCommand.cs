using EnterpriseERP.Application.Features.Workflows.DTOs;
using EnterpriseERP.Domain.Entities.Workflow;
using MediatR;

namespace EnterpriseERP.Application.Features.Workflows.Commands.ActOnApprovalRequest;

public class ActOnApprovalRequestCommand : IRequest<ApprovalRequestDto>
{
    public Guid ApprovalRequestId { get; set; }
    public ApprovalActionType ActionType { get; set; }
    public string ActorUserId { get; set; } = string.Empty;
    public string? Comment { get; set; }
}
