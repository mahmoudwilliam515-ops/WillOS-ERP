using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Workflows.DTOs;
using EnterpriseERP.Domain.Entities.Workflow;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Workflows.Commands.SubmitApprovalRequest;

public class SubmitApprovalRequestCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitApprovalRequestCommand, ApprovalRequestDto>
{
    public async Task<ApprovalRequestDto> Handle(SubmitApprovalRequestCommand request, CancellationToken cancellationToken)
    {
        var existingOpen = (await unitOfWork.Repository<ApprovalRequest>().FindAsync(a =>
                a.DocumentType == request.DocumentType &&
                a.DocumentId == request.DocumentId &&
                a.Status == ApprovalRequestStatus.Pending))
            .Any();

        if (existingOpen)
        {
            throw new AccountingDomainException("A pending approval request already exists for this document.");
        }

        var definitions = await unitOfWork.Repository<WorkflowDefinition>().FindAsync(d =>
            d.DocumentType == request.DocumentType &&
            d.IsActive &&
            d.MinAmount <= request.Amount &&
            (!d.MaxAmount.HasValue || d.MaxAmount.Value >= request.Amount));

        var definition = definitions
            .OrderByDescending(d => d.MinAmount)
            .FirstOrDefault();

        if (definition == null)
        {
            throw new AccountingDomainException("No active workflow definition matches this document.");
        }

        var approval = new ApprovalRequest
        {
            Id = Guid.NewGuid(),
            WorkflowDefinitionId = definition.Id,
            DocumentType = request.DocumentType,
            DocumentId = request.DocumentId,
            DocumentNumber = request.DocumentNumber,
            Amount = request.Amount,
            RequestedBy = request.RequestedBy,
            RequestedAt = DateTime.UtcNow,
            Status = ApprovalRequestStatus.Pending,
            RequiredApprovals = definition.RequiredApprovals,
            Notes = request.Notes ?? string.Empty
        };

        await unitOfWork.Repository<ApprovalRequest>().AddAsync(approval);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(approval);
    }

    internal static ApprovalRequestDto ToDto(ApprovalRequest approval) => new()
    {
        Id = approval.Id,
        WorkflowDefinitionId = approval.WorkflowDefinitionId,
        DocumentType = approval.DocumentType,
        DocumentId = approval.DocumentId,
        DocumentNumber = approval.DocumentNumber,
        Amount = approval.Amount,
        RequestedBy = approval.RequestedBy,
        RequestedAt = approval.RequestedAt,
        Status = approval.Status,
        RequiredApprovals = approval.RequiredApprovals,
        ApprovalCount = approval.ApprovalCount,
        CompletedAt = approval.CompletedAt,
        DueDate = approval.DueDate,
        IsEscalated = approval.IsEscalated,
        DelegatedToUserId = approval.DelegatedToUserId
    };
}
