using EnterpriseERP.Application.Features.Inventory.Queries.GetAllCycleCounts;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryCountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryCountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.InventoryAdvancedView)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetAllCycleCountsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });

        return Ok(new
        {
            Success = true,
            Message = "Cycle counts retrieved successfully",
            Data = result,
            Errors = Array.Empty<string>()
        });
    }
}
