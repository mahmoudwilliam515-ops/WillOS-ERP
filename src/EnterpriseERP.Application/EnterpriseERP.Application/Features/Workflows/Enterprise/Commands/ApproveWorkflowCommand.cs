using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Workflow;
using MediatR;

namespace EnterpriseERP.Application.Features.Workflows.Enterprise.Commands;

public class ApproveWorkflowCommand : IRequest<ApproveWorkflowResponse>
{
    public Guid WorkflowId { get; set; }
    public string? Comments { get; set; }
    public string? DigitalSignature { get; set; }
}

public class ApproveWorkflowResponse
{
    public Guid TaskId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public short? NextLevel { get; set; }
    public string? WorkflowStatus { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public class ApproveWorkflowCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<ApproveWorkflowCommand, ApproveWorkflowResponse>
{
    public async Task<ApproveWorkflowResponse> Handle(ApproveWorkflowCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId != null
            ? Guid.Parse(currentUserService.UserId)
            : Guid.Empty;

        var workflow = await unitOfWork.Repository<WorkflowInstance>()
            .GetByIdAsync(request.WorkflowId);

        if (workflow == null)
            throw new Exception("Workflow not found");

        // Get current pending task for this user and level
        var tasks = await unitOfWork.Repository<WorkflowTask>()
            .FindAsync(t => t.WorkflowInstanceId == request.WorkflowId
                           && t.AssigneeId == userId
                           && t.TaskStatus == WorkflowTaskStatus.PENDING
                           && t.LevelNumber == workflow.CurrentLevel);
        var currentTask = tasks.FirstOrDefault();

        if (currentTask == null)
            throw new Exception("No pending task found for current user at this workflow level");

        var oldStatus = workflow.Status;

        // Update task
        currentTask.TaskStatus = WorkflowTaskStatus.APPROVED;
        currentTask.Decision = "APPROVED";
        currentTask.Comments = request.Comments;
        currentTask.DecidedAt = DateTimeOffset.UtcNow;
        unitOfWork.Repository<WorkflowTask>().Update(currentTask);

        // Immutable audit entry
        await unitOfWork.Repository<WorkflowAuditLog>().AddAsync(new WorkflowAuditLog
        {
            CompanyId = workflow.CompanyId,
            WorkflowInstanceId = workflow.Id,
            TaskId = currentTask.Id,
            EventType = "TASK_APPROVED",
            ActorId = userId,
            OldStatus = oldStatus.ToString(),
            NewStatus = WorkflowInstanceStatus.APPROVED.ToString(),
            EventPayload = System.Text.Json.JsonSerializer.Serialize(new { request.Comments })
        });

        // Final level → COMPLETED
        if (workflow.CurrentLevel >= workflow.TotalLevels)
        {
            workflow.Status = WorkflowInstanceStatus.APPROVED;
            workflow.CompletedAt = DateTimeOffset.UtcNow;
            unitOfWork.Repository<WorkflowInstance>().Update(workflow);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new ApproveWorkflowResponse
            {
                TaskId = currentTask.Id,
                WorkflowStatus = "COMPLETED",
                CompletedAt = workflow.CompletedAt
            };
        }

        // Advance to next level
        workflow.CurrentLevel++;
        workflow.Status = WorkflowInstanceStatus.IN_REVIEW;
        unitOfWork.Repository<WorkflowInstance>().Update(workflow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApproveWorkflowResponse
        {
            TaskId = currentTask.Id,
            NewStatus = "APPROVED",
            NextLevel = workflow.CurrentLevel
        };
    }
}
