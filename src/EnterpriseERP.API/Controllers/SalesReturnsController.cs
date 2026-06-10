using EnterpriseERP.Application.Features.SalesReturns.Commands.ApproveSalesReturn;
using EnterpriseERP.Application.Features.SalesReturns.Commands.CreateSalesReturn;
using EnterpriseERP.Application.Features.SalesReturns.Queries.GetAllSalesReturns;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SalesReturnsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalesReturnsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.SalesReturnsView)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetAllSalesReturnsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.SalesReturnsCreate)]
    public async Task<IActionResult> Create([FromBody] CreateSalesReturnCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = result.IsSuccess, data = result.IsSuccess ? result.Value : Guid.Empty, message = result.IsSuccess ? "" : result.Error.Message });
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission(Permissions.SalesReturnsApprove)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveSalesReturnCommand(id));
        return Ok(new { success = result.IsSuccess, message = result.IsSuccess ? "Sales Return approved, inventory restocked and accounting entry created." : result.Error.Message });
    }
}
