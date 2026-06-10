using System.Linq;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Workflows.Commands.SubmitApprovalRequest;
using EnterpriseERP.Application.Features.Workflows.DTOs;
using EnterpriseERP.Domain.Entities.Workflow;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Workflows.Commands.ActOnApprovalRequest;

public class ActOnApprovalRequestCommandHandler
    : IRequestHandler<ActOnApprovalRequestCommand, ApprovalRequestDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActOnApprovalRequestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApprovalRequestDto> Handle(ActOnApprovalRequestCommand request, CancellationToken cancellationToken)
    {
        var approval = await _unitOfWork.Repository<ApprovalRequest>().GetByIdAsync(request.ApprovalRequestId);
        if (approval == null)
            throw new AccountingDomainException("Approval request was not found.");

        if (approval.Status != ApprovalRequestStatus.Pending)
            throw new AccountingDomainException("Only pending approval requests can be acted on.");

        var existingActions = await _unitOfWork.Repository<ApprovalAction>().FindAsync(a =>
            a.ApprovalRequestId == request.ApprovalRequestId &&
            a.ActorUserId == request.ActorUserId);

        if (existingActions.Any())
            throw new AccountingDomainException("This approver cannot act on the same request more than once.");

        var action = new ApprovalAction
        {
            Id = Guid.NewGuid(),
            ApprovalRequestId = approval.Id,
            ActionType = request.ActionType,
            ActorUserId = request.ActorUserId,
            Comment = request.Comment ?? string.Empty,
            ActionAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ApprovalAction>().AddAsync(action);

        if (request.ActionType == ApprovalActionType.Rejected)
        {
            approval.Status = ApprovalRequestStatus.Rejected;
            approval.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            approval.ApprovalCount++;
            if (approval.ApprovalCount >= approval.RequiredApprovals)
            {
                approval.Status = ApprovalRequestStatus.Approved;
                approval.CompletedAt = DateTime.UtcNow;
            }
        }

        _unitOfWork.Repository<ApprovalRequest>().Update(approval);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SubmitApprovalRequestCommandHandler.ToDto(approval);
    }
}
