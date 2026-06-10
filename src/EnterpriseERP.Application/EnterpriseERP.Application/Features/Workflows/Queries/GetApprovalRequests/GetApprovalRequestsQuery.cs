using EnterpriseERP.Application.Features.Workflows.DTOs;
using EnterpriseERP.Domain.Entities.Workflow;
using MediatR;

namespace EnterpriseERP.Application.Features.Workflows.Queries.GetApprovalRequests;

public class GetApprovalRequestsQuery : IRequest<IEnumerable<ApprovalRequestDto>>
{
    public ApprovalRequestStatus? Status { get; set; }
    public WorkflowDocumentType? DocumentType { get; set; }
}
