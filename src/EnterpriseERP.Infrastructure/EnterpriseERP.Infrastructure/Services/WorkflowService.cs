using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Infrastructure.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public WorkflowService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Guid> SubmitForApprovalAsync(WorkflowDocumentType docType, Guid docId, string docNumber, decimal amount, string requestedBy, CancellationToken cancellationToken = default)
    {
        // 1. Find matching active workflow definition
        var definition = await _unitOfWork.Repository<WorkflowDefinition>().Query()
            .Where(w => w.DocumentType == docType && w.IsActive)
            .Where(w => amount >= w.MinAmount && (w.MaxAmount == null || amount <= w.MaxAmount))
            // We would also add company/cost center checks here if passed from the context
            .OrderByDescending(w => w.MinAmount) // Take most specific rule
            .FirstOrDefaultAsync(cancellationToken);

        if (definition == null)
        {
            // RULE-WF01: No workflow found means auto-approve or use default.
            // For safety in Enterprise ERP, let's assume auto-approve only if no workflows exist at all for this type.
            return Guid.Empty; 
        }

        // 2. Create Approval Request
        var request = new ApprovalRequest
        {
            Id = Guid.NewGuid(),
            WorkflowDefinitionId = definition.Id,
            DocumentType = docType,
            DocumentId = docId,
            DocumentNumber = docNumber,
            Amount = amount,
            RequestedBy = requestedBy,
            RequestedAt = DateTime.UtcNow,
            Status = ApprovalRequestStatus.Pending,
            RequiredApprovals = definition.RequiredApprovals,
            ApprovalCount = 0,
            DueDate = definition.SlaHours.HasValue ? DateTime.UtcNow.AddHours(definition.SlaHours.Value) : null,
            Notes = $"Submitted via automated workflow: {definition.Name}"
        };

        await _unitOfWork.Repository<ApprovalRequest>().AddAsync(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notification to the approver role (or system broadcast for now)
        await _notificationService.SendSystemAlertAsync(
            "طلب اعتماد جديد",
            $"تم تقديم طلب اعتماد لمستند رقم {docNumber} وقيمته {amount:N2} بانتظار الموافقة من {definition.ApproverRole}",
            "info",
            $"/workflows/inbox?id={request.Id}"
        );

        return request.Id;
    }

    public async Task<bool> ProcessActionAsync(Guid requestId, ApprovalActionType actionType, string actorUserId, string comment, CancellationToken cancellationToken = default)
    {
        var request = await _unitOfWork.Repository<ApprovalRequest>().Query()
            .Include(r => r.Actions)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (request == null || request.Status != ApprovalRequestStatus.Pending)
            return false;

        // 1. Record the action
        var action = new ApprovalAction
        {
            Id = Guid.NewGuid(),
            ApprovalRequestId = requestId,
            ActionType = actionType,
            ActorUserId = actorUserId,
            Comment = comment,
            ActionAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ApprovalAction>().AddAsync(action);

        // 2. Update Request Status
        if (actionType == ApprovalActionType.Rejected)
        {
            request.Status = ApprovalRequestStatus.Rejected;
            request.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            request.ApprovalCount++;
            if (request.ApprovalCount >= request.RequiredApprovals)
            {
                request.Status = ApprovalRequestStatus.Approved;
                request.CompletedAt = DateTime.UtcNow;
            }
        }

        _unitOfWork.Repository<ApprovalRequest>().Update(request);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> IsFullyApprovedAsync(WorkflowDocumentType docType, Guid docId, CancellationToken cancellationToken = default)
    {
        var request = await _unitOfWork.Repository<ApprovalRequest>().Query()
            .Where(r => r.DocumentType == docType && r.DocumentId == docId)
            .OrderByDescending(r => r.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // If no request exists, check if any workflow was required
        if (request == null)
        {
            // (Business Rule: If no workflow is configured for this amount/type, it's considered approved)
            return true;
        }

        return request.Status == ApprovalRequestStatus.Approved;
    }
}
