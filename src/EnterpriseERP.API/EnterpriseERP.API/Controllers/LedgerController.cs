using EnterpriseERP.Application.Features.Accounting.Queries.GetAccountLedger;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LedgerController : ControllerBase
{
    private readonly IMediator _mediator;

    public LedgerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("party/{partyId}")]
    [HasPermission(Permissions.LedgerView)]
    public async Task<IActionResult> GetPartyLedger(Guid partyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var result = await _mediator.Send(new GetAccountLedgerQuery(partyId, page, pageSize));
        return Ok(new { success = true, data = result });
    }
}
