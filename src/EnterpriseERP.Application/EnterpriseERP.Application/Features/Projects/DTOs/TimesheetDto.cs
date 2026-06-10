using EnterpriseERP.Domain.Entities.Projects;

namespace EnterpriseERP.Application.Features.Projects.DTOs;

public record TimesheetDto
{
    public Guid Id { get; init; }
    public string TimesheetNumber { get; init; } = string.Empty;
    public string EmployeeId { get; init; } = string.Empty;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public TimesheetStatus Status { get; init; }
    public decimal TotalHours { get; init; }
    public decimal TotalLaborCost { get; init; }
    public IReadOnlyCollection<TimesheetLineDto> Lines { get; init; } = Array.Empty<TimesheetLineDto>();
}

public record TimesheetLineDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid ProjectTaskId { get; init; }
    public DateTime WorkDate { get; init; }
    public decimal Hours { get; init; }
    public decimal HourlyRate { get; init; }
    public decimal LaborCost { get; init; }
    public string Description { get; init; } = string.Empty;
}
