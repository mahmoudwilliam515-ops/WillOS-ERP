using EnterpriseERP.Application.Features.Branches.Commands.CreateBranch;
using EnterpriseERP.Application.Features.Branches.Queries.GetAllBranches;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BranchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.BranchesView)]
    public async Task<IActionResult> GetAll()
    {
        var branches = await _mediator.Send(new GetAllBranchesQuery());
        
        return Ok(new
        {
            Success = true,
            Message = "Branches retrieved successfully",
            Data = branches,
            Errors = Array.Empty<string>()
        });
    }

    [HttpPost]
    [HasPermission(Permissions.BranchesCreate)]
    public async Task<IActionResult> Create([FromBody] CreateBranchCommand command)
    {
        var branch = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetAll), new { id = branch.Id }, new
        {
            Success = true,
            Message = "Branch created successfully",
            Data = branch,
            Errors = Array.Empty<string>()
        });
    }
}
