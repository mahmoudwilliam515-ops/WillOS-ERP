using EnterpriseERP.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using EnterpriseERP.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;
using EnterpriseERP.Application.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.PurchaseInvoicesView)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllPurchaseOrdersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.PurchaseInvoicesCreate)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(Create), new { id = result.Value }, new { success = true, data = result.Value, message = "Purchase order created successfully." });
            
        return BadRequest(new { success = false, message = result.Error.Message });
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission(Permissions.PurchaseInvoicesCreate)] // Using same permission for now
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApprovePurchaseOrderCommand(id));
        return Ok(new { success = result, message = result ? "Purchase order approved and commitments recorded." : "Approval failed." });
    }
}
