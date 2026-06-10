using EnterpriseERP.Application.Features.PurchaseReturns.Commands.ApprovePurchaseReturn;
using EnterpriseERP.Application.Features.PurchaseReturns.Commands.CreatePurchaseReturn;
using EnterpriseERP.Application.Features.PurchaseReturns.Queries.GetAllPurchaseReturns;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PurchaseReturnsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PurchaseReturnsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.PurchaseReturnsView)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetAllPurchaseReturnsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.PurchaseReturnsCreate)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseReturnCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission(Permissions.PurchaseReturnsApprove)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ok = await _mediator.Send(new ApprovePurchaseReturnCommand(id));
        if (!ok)
            return NotFound(new { success = false, message = "Return not found or already approved." });
        return Ok(new { success = true });
    }
}
