using EnterpriseERP.Application.Features.Projects.DTOs;
using EnterpriseERP.Domain.Entities.Projects;

namespace EnterpriseERP.Application.Features.Projects;

internal static class ProjectMapper
{
    internal static ProjectDto ToDto(Project project) => new()
    {
        Id = project.Id,
        Code = project.Code,
        Name = project.Name,
        CustomerName = project.CustomerName,
        CostCenterId = project.CostCenterId,
        StartDate = project.StartDate,
        EndDate = project.EndDate,
        Status = project.Status,
        BudgetAmount = project.BudgetAmount,
        ActualLaborCost = project.ActualLaborCost
    };

    internal static ProjectTaskDto ToDto(ProjectTask task) => new()
    {
        Id = task.Id,
        ProjectId = task.ProjectId,
        ParentTaskId = task.ParentTaskId,
        WbsCode = task.WbsCode,
        Name = task.Name,
        Status = task.Status,
        PlannedHours = task.PlannedHours,
        ActualHours = task.ActualHours,
        ActualLaborCost = task.ActualLaborCost
    };

    internal static TimesheetDto ToDto(Timesheet timesheet) => new()
    {
        Id = timesheet.Id,
        TimesheetNumber = timesheet.TimesheetNumber,
        EmployeeId = timesheet.EmployeeId,
        PeriodStart = timesheet.PeriodStart,
        PeriodEnd = timesheet.PeriodEnd,
        Status = timesheet.Status,
        TotalHours = timesheet.TotalHours,
        TotalLaborCost = timesheet.TotalLaborCost,
        Lines = timesheet.Lines.Select(l => new TimesheetLineDto
        {
            Id = l.Id,
            ProjectId = l.ProjectId,
            ProjectTaskId = l.ProjectTaskId,
            WorkDate = l.WorkDate,
            Hours = l.Hours,
            HourlyRate = l.HourlyRate,
            LaborCost = l.LaborCost,
            Description = l.Description
        }).ToList()
    };
}
