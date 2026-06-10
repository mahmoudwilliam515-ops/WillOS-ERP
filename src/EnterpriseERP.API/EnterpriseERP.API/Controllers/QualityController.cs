using EnterpriseERP.Application.Features.Quality.Commands.PerformInspection;
using EnterpriseERP.Application.Features.Quality.Commands.RequestInspection;
using EnterpriseERP.Application.Features.Quality.Queries.GetChecklists;
using EnterpriseERP.Application.Features.Quality.Queries.GetInspectionsByReference;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Domain.Entities.Quality;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QualityController : ControllerBase
{
    private readonly IMediator _mediator;

    public QualityController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("checklists")]
    [HasPermission(Permissions.InventoryView)]
    public async Task<IActionResult> GetChecklists()
    {
        var result = await _mediator.Send(new GetChecklistsQuery());
        return Ok(new { success = result.IsSuccess, data = result.Value });
    }

    [HttpGet("inspections/reference/{referenceId:guid}")]
    [HasPermission(Permissions.InventoryView)]
    public async Task<IActionResult> GetInspectionsByReference(Guid referenceId, [FromQuery] InspectionType type)
    {
        var result = await _mediator.Send(new GetInspectionsByReferenceQuery(referenceId, type));
        return Ok(new { success = result.IsSuccess, data = result.Value });
    }

    [HttpPost("inspections/request")]
    [HasPermission(Permissions.InventoryManage)]
    public async Task<IActionResult> RequestInspection([FromBody] RequestInspectionCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = result.IsSuccess, data = result.Value, message = result.IsSuccess ? "Inspection requested" : result.Error.Message });
    }

    [HttpPost("inspections/perform")]
    [HasPermission(Permissions.InventoryManage)]
    public async Task<IActionResult> PerformInspection([FromBody] PerformInspectionCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { success = result.IsSuccess, data = result.Value, message = result.IsSuccess ? "Inspection completed" : result.Error.Message });
    }
}
