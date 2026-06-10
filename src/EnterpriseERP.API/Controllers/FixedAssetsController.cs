using EnterpriseERP.Application.Features.FixedAssets.Commands.CreateFixedAsset;
using EnterpriseERP.Application.Features.FixedAssets.Commands.ProcessDepreciation;
using EnterpriseERP.Application.Features.FixedAssets.Queries.GetFixedAssets;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixedAssetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FixedAssetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.FixedAssetsView)]
    public async Task<IActionResult> Get([FromQuery] GetFixedAssetsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.FixedAssetsCreate)]
    public async Task<IActionResult> Create([FromBody] CreateFixedAssetCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id }, new { Success = true, Message = "Fixed asset created successfully", Data = id });
    }

    [HttpPost("depreciation/process")]
    [HasPermission(Permissions.FixedAssetsDepreciate)]
    public async Task<IActionResult> ProcessDepreciation([FromBody] ProcessDepreciationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { Success = true, Data = result, Message = "Depreciation processed successfully" });
    }
}
