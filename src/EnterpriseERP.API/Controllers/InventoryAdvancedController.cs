using EnterpriseERP.Application.Features.Inventory.Commands.CreateInventoryTransfer;
using EnterpriseERP.Application.Features.Inventory.Commands.CreateInventoryAdjustment;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryAdvancedController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryAdvancedController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Create a new inter-warehouse inventory transfer.</summary>
    [HttpPost("transfers")]
    [HasPermission(Permissions.InventoryAdvancedManage)]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateInventoryTransferCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(CreateTransfer), new { id = result.Value },
                new { success = true, data = result.Value, message = "Transfer created." });

        return BadRequest(new { success = false, message = result.Error.Message });
    }

    /// <summary>Create a stock adjustment (increase/decrease/damage/theft/found).</summary>
    [HttpPost("adjustments")]
    [HasPermission(Permissions.InventoryAdvancedManage)]
    public async Task<IActionResult> CreateAdjustment([FromBody] CreateInventoryAdjustmentCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(CreateAdjustment), new { id = result.Value },
                new { success = true, data = result.Value, message = "Adjustment created." });

        return BadRequest(new { success = false, message = result.Error.Message });
    }

    /// <summary>Reserve stock for an order to prevent overselling.</summary>
    [HttpPost("reservations")]
    [HasPermission(Permissions.InventoryAdvancedManage)]
    public async Task<IActionResult> CreateReservation(
        [FromBody] EnterpriseERP.Application.Features.Inventory.Commands.CreateInventoryReservation.CreateInventoryReservationCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return Ok(new { success = true, data = result.Value, message = "Reservation created." });

        return BadRequest(new { success = false, message = result.Error.Message });
    }

    /// <summary>Submit a cycle count (physical stock count) for a warehouse.</summary>
    [HttpPost("cycle-counts")]
    [HasPermission(Permissions.InventoryAdvancedManage)]
    public async Task<IActionResult> CreateCycleCount(
        [FromBody] EnterpriseERP.Application.Features.Inventory.Commands.CreateCycleCount.CreateCycleCountCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return Ok(new { success = true, data = result.Value, message = "Cycle count recorded." });

        return BadRequest(new { success = false, message = result.Error.Message });
    }
    /// <summary>Get list of inventory transfers.</summary>
    [HttpGet("transfers")]
    [HasPermission(Permissions.InventoryAdvancedView)]
    public async Task<IActionResult> GetTransfers([FromQuery] EnterpriseERP.Application.Features.Inventory.Queries.GetAllInventoryTransfers.GetAllInventoryTransfersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { success = true, data = result, message = "Transfers retrieved successfully." });
    }

    /// <summary>Get list of inventory adjustments.</summary>
    [HttpGet("adjustments")]
    [HasPermission(Permissions.InventoryAdvancedView)]
    public async Task<IActionResult> GetAdjustments([FromQuery] EnterpriseERP.Application.Features.Inventory.Queries.GetAllInventoryAdjustments.GetAllInventoryAdjustmentsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { success = true, data = result, message = "Adjustments retrieved successfully." });
    }
}
