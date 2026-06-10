using EnterpriseERP.Application.Features.HR.Commands.CreateEmployee;
using EnterpriseERP.Application.Features.HR.Queries.GetEmployees;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.EmployeesView)]
    public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
    {
        var query = new GetEmployeesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(new { Success = true, Data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.EmployeesCreate)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { Success = true, Data = id, Message = "Employee created successfully." });
    }
}
