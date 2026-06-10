using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Workflows.Commands.SubmitApprovalRequest;
using EnterpriseERP.Application.Features.Workflows.DTOs;
using EnterpriseERP.Domain.Entities.Workflow;
using MediatR;

namespace EnterpriseERP.Application.Features.Workflows.Queries.GetApprovalRequests;

public class GetApprovalRequestsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetApprovalRequestsQuery, IEnumerable<ApprovalRequestDto>>
{
    public async Task<IEnumerable<ApprovalRequestDto>> Handle(GetApprovalRequestsQuery request, CancellationToken cancellationToken)
    {
        var approvals = await unitOfWork.Repository<ApprovalRequest>().GetAllAsync();

        return approvals
            .Where(a => !request.Status.HasValue || a.Status == request.Status.Value)
            .Where(a => !request.DocumentType.HasValue || a.DocumentType == request.DocumentType.Value)
            .OrderByDescending(a => a.RequestedAt)
            .Select(SubmitApprovalRequestCommandHandler.ToDto)
            .ToList();
    }
}
