using EnterpriseERP.Application.Features.Projects.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Commands.CreateTimesheet;

public class CreateTimesheetCommand : IRequest<TimesheetDto>
{
    public string TimesheetNumber { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<CreateTimesheetLineRequest> Lines { get; set; } = new();
}

public class CreateTimesheetLineRequest
{
    public Guid ProjectId { get; set; }
    public Guid ProjectTaskId { get; set; }
    public DateTime WorkDate { get; set; }
    public decimal Hours { get; set; }
    public decimal HourlyRate { get; set; }
    public string Description { get; set; } = string.Empty;
}
