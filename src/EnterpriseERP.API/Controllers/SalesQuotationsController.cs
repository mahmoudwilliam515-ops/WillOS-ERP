using EnterpriseERP.Application.Features.SalesQuotations.Commands.CreateSalesQuotation;
using EnterpriseERP.Application.Features.SalesQuotations.Queries.GetAllSalesQuotations;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesQuotationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalesQuotationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.SalesInvoicesView)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllSalesQuotationsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.SalesInvoicesCreate)]
    public async Task<IActionResult> Create([FromBody] CreateSalesQuotationCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(Create), new { id = result.Value }, new { success = true, data = result.Value, message = "Quotation created successfully." });
            
        return BadRequest(new { success = false, message = result.Error.Message });
    }
}
