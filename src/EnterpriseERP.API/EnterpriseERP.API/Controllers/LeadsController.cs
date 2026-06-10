using EnterpriseERP.Application.Features.CRM.Leads.Commands.CreateLead;
using EnterpriseERP.Application.Features.CRM.Leads.Queries.GetAllLeads;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeadsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.LeadsView)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllLeadsQuery(page, pageSize));
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.LeadsCreate)]
    public async Task<IActionResult> Create([FromBody] CreateLeadCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = true, data = result });
    }
}
