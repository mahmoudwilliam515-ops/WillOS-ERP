using EnterpriseERP.Application.Features.Inventory.Commands.ShipTransfer;
using EnterpriseERP.Application.Features.Inventory.Commands.ReceiveTransfer;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[HasPermission(Permissions.InventoryView)]
public class TransferOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransferOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{id:guid}/ship")]
    [HasPermission(Permissions.InventoryManage)]
    public async Task<IActionResult> Ship(Guid id)
    {
        var result = await _mediator.Send(new ShipTransferCommand(id));
        return Ok(new { success = result, message = result ? "Transfer shipped and moved to transit." : "Shipping failed." });
    }

    [HttpPost("{id:guid}/receive")]
    [HasPermission(Permissions.InventoryManage)]
    public async Task<IActionResult> Receive(Guid id)
    {
        var result = await _mediator.Send(new ReceiveTransferCommand(id));
        return Ok(new { success = result, message = result ? "Transfer received into destination warehouse." : "Receiving failed." });
    }
}
