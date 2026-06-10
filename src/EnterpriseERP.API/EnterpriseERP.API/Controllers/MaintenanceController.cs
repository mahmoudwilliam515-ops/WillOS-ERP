using EnterpriseERP.Application.Features.Maintenance.Commands.CreateWorkOrder;
using EnterpriseERP.Application.Features.Maintenance.Commands.StartWorkOrder;
using EnterpriseERP.Application.Features.Maintenance.Commands.CompleteWorkOrder;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public MaintenanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("workorders")]
    [HasPermission(Permissions.MaintenanceView)]
    public async Task<IActionResult> GetWorkOrders()
    {
        var result = await _mediator.Send(new EnterpriseERP.Application.Features.Maintenance.Queries.GetWorkOrders.GetMaintenanceWorkOrdersQuery());
        if (result.IsSuccess)
            return Ok(new { success = true, data = result.Value });

        return BadRequest(new { success = false, message = result.Error.Message });
    }

    /// <summary>Create a new maintenance work order.</summary>
    [HttpPost("workorders")]
    [HasPermission(Permissions.MaintenanceManage)]
    public async Task<IActionResult> CreateWorkOrder([FromBody] CreateMaintenanceWorkOrderCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(CreateWorkOrder), new { id = result.Value },
                new { success = true, data = result.Value, message = "Maintenance work order created." });

        return BadRequest(new { success = false, message = result.Error.Message });
    }

    /// <summary>Start work on a maintenance work order (Open → InProgress).</summary>
    [HttpPost("workorders/{id:guid}/start")]
    [HasPermission(Permissions.MaintenanceManage)]
    public async Task<IActionResult> StartWorkOrder(Guid id)
    {
        var result = await _mediator.Send(new StartWorkOrderCommand { WorkOrderId = id });
        if (result.IsSuccess)
            return Ok(new { success = true, message = "Work order started." });

        return BadRequest(new { success = false, message = result.Error.Message });
    }

    /// <summary>Complete a maintenance work order (InProgress → Completed).</summary>
    [HttpPost("workorders/{id:guid}/complete")]
    [HasPermission(Permissions.MaintenanceManage)]
    public async Task<IActionResult> CompleteWorkOrder(Guid id, [FromBody] CompleteWorkOrderRequest request)
    {
        var result = await _mediator.Send(new CompleteWorkOrderCommand
        {
            WorkOrderId       = id,
            ResolutionNotes   = request.ResolutionNotes,
            TotalPartsCost    = request.TotalPartsCost,
            TotalLaborCost    = request.TotalLaborCost
        });

        if (result.IsSuccess)
            return Ok(new { success = true, message = "Work order completed." });

        return BadRequest(new { success = false, message = result.Error.Message });
    }
}

/// <summary>Request body for completing a maintenance work order.</summary>
public record CompleteWorkOrderRequest(
    string ResolutionNotes,
    decimal TotalPartsCost,
    decimal TotalLaborCost
);
