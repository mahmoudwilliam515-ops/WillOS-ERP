using EnterpriseERP.Application.Features.Projects.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommand : IRequest<ProjectDto>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public Guid? CostCenterId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? EndDate { get; set; }
    public decimal BudgetAmount { get; set; }
    public string Description { get; set; } = string.Empty;
}
