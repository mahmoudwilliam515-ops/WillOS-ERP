using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Workflow;
using MediatR;

namespace EnterpriseERP.Application.Features.Workflows.Enterprise.Commands;

public class RejectWorkflowCommand : IRequest<RejectWorkflowResponse>
{
    public Guid WorkflowId { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string RejectionCode { get; set; } = string.Empty;
}

public class RejectWorkflowResponse
{
    public Guid TaskId { get; set; }
    public string WorkflowStatus { get; set; } = string.Empty;
    public DateTimeOffset RejectedAt { get; set; }
    public bool ReturnToRequester { get; set; }
}

public class RejectWorkflowCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<RejectWorkflowCommand, RejectWorkflowResponse>
{
    public async Task<RejectWorkflowResponse> Handle(RejectWorkflowCommand request, CancellationToken cancellationToken)
    {
        // SOX: Comments are mandatory on rejection
        if (string.IsNullOrWhiteSpace(request.Comments))
            throw new InvalidOperationException("WF-REJ-001: Comments are mandatory on rejection (SOX compliance)");

        var userId = currentUserService.UserId != null
            ? Guid.Parse(currentUserService.UserId)
            : Guid.Empty;

        var workflow = await unitOfWork.Repository<WorkflowInstance>()
            .GetByIdAsync(request.WorkflowId);

        if (workflow == null)
            throw new Exception("Workflow not found");

        var tasks = await unitOfWork.Repository<WorkflowTask>()
            .FindAsync(t => t.WorkflowInstanceId == request.WorkflowId
                           && t.AssigneeId == userId
                           && t.TaskStatus == WorkflowTaskStatus.PENDING
                           && t.LevelNumber == workflow.CurrentLevel);
        var currentTask = tasks.FirstOrDefault();

        if (currentTask == null)
            throw new Exception("No pending task found for current user at this workflow level");

        var oldStatus = workflow.Status;
        var decidedAt = DateTimeOffset.UtcNow;

        currentTask.TaskStatus = WorkflowTaskStatus.REJECTED;
        currentTask.Decision = "REJECTED";
        currentTask.Comments = request.Comments;
        currentTask.DecidedAt = decidedAt;
        unitOfWork.Repository<WorkflowTask>().Update(currentTask);

        workflow.Status = WorkflowInstanceStatus.REJECTED;
        unitOfWork.Repository<WorkflowInstance>().Update(workflow);

        // Immutable audit log — TASK_REJECTED
        await unitOfWork.Repository<WorkflowAuditLog>().AddAsync(new WorkflowAuditLog
        {
            CompanyId = workflow.CompanyId,
            WorkflowInstanceId = workflow.Id,
            TaskId = currentTask.Id,
            EventType = "TASK_REJECTED",
            ActorId = userId,
            OldStatus = oldStatus.ToString(),
            NewStatus = workflow.Status.ToString(),
            EventPayload = System.Text.Json.JsonSerializer.Serialize(new
            {
                RejectionCode = request.RejectionCode,
                Comments = request.Comments
            })
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RejectWorkflowResponse
        {
            TaskId = currentTask.Id,
            WorkflowStatus = "REJECTED",
            RejectedAt = decidedAt,
            ReturnToRequester = true
        };
    }
}
