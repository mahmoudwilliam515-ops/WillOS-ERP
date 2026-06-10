using EnterpriseERP.Application.Features.Projects.Commands.ApproveTimesheet;
using EnterpriseERP.Application.Features.Projects.Commands.CreateProject;
using EnterpriseERP.Application.Features.Projects.Commands.CreateProjectTask;
using EnterpriseERP.Application.Features.Projects.Commands.CreateTimesheet;
using EnterpriseERP.Application.Features.Projects.Commands.RejectTimesheet;
using EnterpriseERP.Application.Features.Projects.Commands.SubmitTimesheet;
using EnterpriseERP.Application.Features.Projects.Queries.GetProjects;
using EnterpriseERP.Application.Features.Projects.Queries.GetProjectTasks;
using EnterpriseERP.Application.Features.Projects.Queries.GetTimesheets;
using EnterpriseERP.Application.Features.Projects.Queries.GetProjectProfitability;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Domain.Entities.Projects;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.ProjectsView)]
    public async Task<IActionResult> GetProjects([FromQuery] ProjectStatus? status)
    {
        var result = await mediator.Send(new GetProjectsQuery { Status = status });
        return Ok(new { Success = true, Message = "Projects retrieved successfully", Data = result, Errors = Array.Empty<string>() });
    }

    [HttpPost]
    [HasPermission(Permissions.ProjectsManage)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand command)
    {
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetProjects), new { id = result.Id }, new { Success = true, Message = "Project created successfully", Data = result, Errors = Array.Empty<string>() });
    }

    [HttpGet("{projectId:guid}/tasks")]
    [HasPermission(Permissions.ProjectsView)]
    public async Task<IActionResult> GetTasks(Guid projectId)
    {
        var result = await mediator.Send(new GetProjectTasksQuery { ProjectId = projectId });
        return Ok(new { Success = true, Message = "Project tasks retrieved successfully", Data = result, Errors = Array.Empty<string>() });
    }

    [HttpPost("{projectId:guid}/tasks")]
    [HasPermission(Permissions.ProjectsManage)]
    public async Task<IActionResult> CreateTask(Guid projectId, [FromBody] CreateProjectTaskCommand command)
    {
        command.ProjectId = projectId;
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetTasks), new { projectId }, new { Success = true, Message = "Project task created successfully", Data = result, Errors = Array.Empty<string>() });
    }

    [HttpGet("timesheets")]
    [HasPermission(Permissions.TimesheetsView)]
    public async Task<IActionResult> GetTimesheets([FromQuery] TimesheetStatus? status, [FromQuery] string? employeeId)
    {
        var result = await mediator.Send(new GetTimesheetsQuery { Status = status, EmployeeId = employeeId });
        return Ok(new { Success = true, Message = "Timesheets retrieved successfully", Data = result, Errors = Array.Empty<string>() });
    }

    [HttpPost("timesheets")]
    [HasPermission(Permissions.TimesheetsManage)]
    public async Task<IActionResult> CreateTimesheet([FromBody] CreateTimesheetCommand command)
    {
        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetTimesheets), new { id = result.Id }, new { Success = true, Message = "Timesheet created successfully", Data = result, Errors = Array.Empty<string>() });
    }

    [HttpPost("timesheets/{timesheetId:guid}/submit")]
    [HasPermission(Permissions.TimesheetsManage)]
    public async Task<IActionResult> SubmitTimesheet(Guid timesheetId, [FromBody] SubmitTimesheetCommand command)
    {
        command.TimesheetId = timesheetId;
        var result = await mediator.Send(command);
        return Ok(new { Success = true, Message = "Timesheet submitted successfully", Data = result, Errors = Array.Empty<string>() });
    }

    [HttpPost("timesheets/{timesheetId:guid}/approve")]
    [HasPermission(Permissions.TimesheetsApprove)]
    public async Task<IActionResult> ApproveTimesheet(Guid timesheetId, [FromBody] ApproveTimesheetCommand command)
    {
        command.TimesheetId = timesheetId;
        var result = await mediator.Send(command);
        return Ok(new { Success = true, Message = "Timesheet approved successfully", Data = result, Errors = Array.Empty<string>() });
    }

    [HttpPost("timesheets/{timesheetId:guid}/reject")]
    [HasPermission(Permissions.TimesheetsApprove)]
    public async Task<IActionResult> RejectTimesheet(Guid timesheetId, [FromBody] RejectTimesheetCommand command)
    {
        command.TimesheetId = timesheetId;
        var result = await mediator.Send(command);
        return Ok(new { Success = true, Message = "Timesheet rejected successfully", Data = result, Errors = Array.Empty<string>() });
    }

    [HttpGet("{projectId:guid}/profitability")]
    [HasPermission(Permissions.ProjectsView)]
    public async Task<IActionResult> GetProfitability(Guid projectId)
    {
        var result = await mediator.Send(new GetProjectProfitabilityQuery(projectId));
        return Ok(new { Success = true, Message = "Project profitability retrieved successfully", Data = result, Errors = Array.Empty<string>() });
    }
}
