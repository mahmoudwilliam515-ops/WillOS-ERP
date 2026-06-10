using EnterpriseERP.Application.Features.Projects.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Commands.CreateProjectTask;

public class CreateProjectTaskCommand : IRequest<ProjectTaskDto>
{
    public Guid ProjectId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public string WbsCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal PlannedHours { get; set; }
}
