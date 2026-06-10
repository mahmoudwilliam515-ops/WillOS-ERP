using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Projects;

public class TimesheetLine : AuditableEntity
{
    public Guid TimesheetId { get; set; }
    public Timesheet Timesheet { get; set; } = null!;
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid ProjectTaskId { get; set; }
    public ProjectTask ProjectTask { get; set; } = null!;
    public DateTime WorkDate { get; set; }
    public decimal Hours { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal LaborCost => Hours * HourlyRate;
    public string Description { get; set; } = string.Empty;
}
