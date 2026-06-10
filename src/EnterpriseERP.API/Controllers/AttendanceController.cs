using EnterpriseERP.Application.Features.HR.Commands.CreateAttendance;
using EnterpriseERP.Application.Features.HR.Queries.GetAllAttendance;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.EmployeesView)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllAttendanceQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [HasPermission(Permissions.EmployeesCreate)]
    public async Task<IActionResult> Create([FromBody] CreateAttendanceCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
            return CreatedAtAction(nameof(Create), new { id = result.Value }, new { success = true, data = result.Value, message = "Attendance record created successfully." });
            
        return BadRequest(new { success = false, message = result.Error.Message });
    }
}
