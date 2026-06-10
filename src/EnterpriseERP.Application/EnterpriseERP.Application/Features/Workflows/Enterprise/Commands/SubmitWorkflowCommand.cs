using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Workflow;
using MediatR;
using System.Text.Json;

namespace EnterpriseERP.Application.Features.Workflows.Enterprise.Commands;

public class SubmitWorkflowCommand : IRequest<SubmitWorkflowResponse>
{
    public string WorkflowType { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public Guid ReferenceId { get; set; }
    public decimal? Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string Priority { get; set; } = "NORMAL";
    public Dictionary<string, object>? Metadata { get; set; }
}

public class SubmitWorkflowResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string WorkflowType { get; set; } = string.Empty;
    public short CurrentLevel { get; set; }
    public short TotalLevels { get; set; }
    public DateTimeOffset SlaDeadline { get; set; }
    public string? TemporalRunId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<ApprovalChainDto> ApprovalChain { get; set; } = new();
}

public class ApprovalChainDto
{
    public short Level { get; set; }
    public Guid ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public short SlaHours { get; set; }
}

public class SubmitWorkflowCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<SubmitWorkflowCommand, SubmitWorkflowResponse>
{
    public async Task<SubmitWorkflowResponse> Handle(SubmitWorkflowCommand request, CancellationToken cancellationToken)
    {
        // SOD Check: requester cannot be approver — enforced at rule evaluation
        var requesterId = currentUserService.UserId != null
            ? Guid.Parse(currentUserService.UserId)
            : Guid.Empty;

        // 1. Resolve approval rules to build chain
        var rules = await unitOfWork.Repository<ApprovalRule>()
            .FindAsync(r =>
                r.WorkflowType == request.WorkflowType &&
                r.IsActive &&
                (r.MinAmount == null || request.Amount == null || request.Amount >= r.MinAmount) &&
                (r.MaxAmount == null || request.Amount == null || request.Amount <= r.MaxAmount));

        var rulesOrdered = rules.OrderBy(r => r.LevelNumber).ToList();
        short totalLevels = (short)(rulesOrdered.Count > 0 ? rulesOrdered.Max(r => r.LevelNumber) : 1);

        // SLA calculated from first level rule (default 24h)
        short firstSlaHours = rulesOrdered.FirstOrDefault()?.SlaHours ?? 24;

        // 2. Create WorkflowInstance
        var workflow = new WorkflowInstance
        {
            CompanyId = currentUserService.CompanyId ?? Guid.Empty,
            WorkflowType = request.WorkflowType,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            RequestedAmount = request.Amount,
            CurrencyCode = request.CurrencyCode,
            RequesterId = requesterId,
            Status = WorkflowInstanceStatus.PENDING_APPROVAL,
            Priority = Enum.TryParse<WorkflowPriority>(request.Priority, out var priority)
                ? priority : WorkflowPriority.NORMAL,
            Metadata = JsonSerializer.Serialize(request.Metadata ?? new Dictionary<string, object>()),
            CurrentLevel = 1,
            TotalLevels = totalLevels,
            SlaDeadline = DateTimeOffset.UtcNow.AddHours(firstSlaHours)
        };

        await unitOfWork.Repository<WorkflowInstance>().AddAsync(workflow);

        // 3. Audit Log — WORKFLOW_STARTED (Immutable)
        var audit = new WorkflowAuditLog
        {
            CompanyId = workflow.CompanyId,
            WorkflowInstanceId = workflow.Id,
            EventType = "WORKFLOW_STARTED",
            ActorId = requesterId,
            NewStatus = workflow.Status.ToString(),
            EventPayload = JsonSerializer.Serialize(new { request.WorkflowType, request.ReferenceType, request.Amount })
        };
        await unitOfWork.Repository<WorkflowAuditLog>().AddAsync(audit);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubmitWorkflowResponse
        {
            Id = workflow.Id,
            Status = workflow.Status.ToString(),
            WorkflowType = workflow.WorkflowType,
            CurrentLevel = workflow.CurrentLevel,
            TotalLevels = workflow.TotalLevels,
            SlaDeadline = workflow.SlaDeadline,
            CreatedAt = workflow.CreatedAt,
            ApprovalChain = rulesOrdered.Select(r => new ApprovalChainDto
            {
                Level = r.LevelNumber,
                Role = r.ApproverRef,
                SlaHours = r.SlaHours
            }).ToList()
        };
    }
}
