using EnterpriseERP.Domain.Entities.Projects;

namespace EnterpriseERP.Application.Features.Projects.DTOs;

public record ProjectTaskDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid? ParentTaskId { get; init; }
    public string WbsCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ProjectTaskStatus Status { get; init; }
    public decimal PlannedHours { get; init; }
    public decimal ActualHours { get; init; }
    public decimal ActualLaborCost { get; init; }
}
