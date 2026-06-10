using EnterpriseERP.Application.Features.HR.Commands.AddDeduction;
using EnterpriseERP.Application.Features.HR.Queries.GetDeductions;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[HasPermission(Permissions.DeductionsManage)]
public class DeductionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeductionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> AddDeduction([FromBody] AddDeductionCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Message = "Deduction added successfully", Data = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetDeductions([FromQuery] GetDeductionsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }
}
