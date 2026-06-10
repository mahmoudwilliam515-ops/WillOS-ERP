using System;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Domain.Entities.Workflow;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IWorkflowService
{
    /// <summary>
    /// Submits a document for approval based on defined workflows.
    /// Returns the ID of the created ApprovalRequest, or Guid.Empty if auto-approved (no workflow found).
    /// </summary>
    Task<Guid> SubmitForApprovalAsync(WorkflowDocumentType docType, Guid docId, string docNumber, decimal amount, string requestedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes an approval or rejection action.
    /// </summary>
    Task<bool> ProcessActionAsync(Guid requestId, ApprovalActionType actionType, string actorUserId, string comment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a document is fully approved.
    /// </summary>
    Task<bool> IsFullyApprovedAsync(WorkflowDocumentType docType, Guid docId, CancellationToken cancellationToken = default);
}
