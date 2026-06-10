using EnterpriseERP.Application.Features.CRM.Opportunities.Commands.CreateOpportunity;
using EnterpriseERP.Application.Features.CRM.Opportunities.Queries.GetAllOpportunities;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OpportunitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public OpportunitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.OpportunitiesView)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllOpportunitiesQuery(page, pageSize));
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.OpportunitiesCreate)]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = true, data = result });
    }
}
