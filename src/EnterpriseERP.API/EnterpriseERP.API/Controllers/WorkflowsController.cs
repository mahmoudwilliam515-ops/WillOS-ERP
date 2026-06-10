using EnterpriseERP.Application.Features.Workflows.Commands.ActOnApprovalRequest;
using EnterpriseERP.Application.Features.Workflows.Commands.CreateWorkflowDefinition;
using EnterpriseERP.Application.Features.Workflows.Commands.SubmitApprovalRequest;
using EnterpriseERP.Application.Features.Workflows.Queries.GetApprovalRequests;
using EnterpriseERP.Application.Features.Workflows.Queries.GetWorkflowDefinitions;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Domain.Entities.Workflow;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using EnterpriseERP.Application.Features.Workflows.Enterprise.Commands;

using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Route("api/v1/workflows")]
public class WorkflowsController(IMediator mediator) : ControllerBase
{
    [HttpGet("definitions")]
    [HasPermission(Permissions.WorkflowsView)]
    public async Task<IActionResult> GetDefinitions([FromQuery] WorkflowDocumentType? documentType, [FromQuery] bool activeOnly = true)
    {
        var result = await mediator.Send(new GetWorkflowDefinitionsQuery
        {
            DocumentType = documentType,
            ActiveOnly = activeOnly
        });

        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("definitions")]
    [HasPermission(Permissions.WorkflowsManage)]
    public async Task<IActionResult> CreateDefinition([FromBody] CreateWorkflowDefinitionCommand command)
    {
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetDefinitions), new { id = result.Id }, new { Success = true, Data = result });
    }

    [HttpGet("approvals")]
    [HasPermission(Permissions.ApprovalsView)]
    public async Task<IActionResult> GetApprovals([FromQuery] ApprovalRequestStatus? status, [FromQuery] WorkflowDocumentType? documentType)
    {
        var result = await mediator.Send(new GetApprovalRequestsQuery
        {
            Status = status,
            DocumentType = documentType
        });

        return Ok(new { Success = true, Data = result });
    }

    [HttpPost("approvals")]
    [HasPermission(Permissions.WorkflowsManage)]
    public async Task<IActionResult> SubmitApproval([FromBody] SubmitApprovalRequestCommand command)
    {
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetApprovals), new { id = result.Id }, new { Success = true, Data = result });
    }

    [HttpPost("approvals/{id:guid}/actions")]
    [HasPermission(Permissions.ApprovalsAct)]
    public async Task<IActionResult> Act(Guid id, [FromBody] ActOnApprovalRequestCommand command)
    {
        command.ApprovalRequestId = id;
        var result = await mediator.Send(command);
        return Ok(new { Success = true, Data = result });
    }

    #region Enterprise Workflow Engine (Section 10)

    [HttpPost]
    [HasPermission(Permissions.WorkflowsManage)]
    public async Task<IActionResult> SubmitEnterpriseWorkflow([FromBody] SubmitWorkflowCommand command)
    {
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(SubmitEnterpriseWorkflow), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission(Permissions.ApprovalsAct)]
    public async Task<IActionResult> ApproveEnterpriseWorkflow(Guid id, [FromBody] ApproveWorkflowCommand command)
    {
        command.WorkflowId = id;
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    [HasPermission(Permissions.ApprovalsAct)]
    public async Task<IActionResult> RejectEnterpriseWorkflow(Guid id, [FromBody] RejectWorkflowCommand command)
    {
        command.WorkflowId = id;
        var result = await mediator.Send(command);
        return Ok(result);
    }

    #endregion
}
