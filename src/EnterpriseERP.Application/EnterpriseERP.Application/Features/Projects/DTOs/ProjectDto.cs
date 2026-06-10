using EnterpriseERP.Domain.Entities.Projects;

namespace EnterpriseERP.Application.Features.Projects.DTOs;

public record ProjectDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public Guid? CostCenterId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public ProjectStatus Status { get; init; }
    public decimal BudgetAmount { get; init; }
    public decimal ActualLaborCost { get; init; }
}
