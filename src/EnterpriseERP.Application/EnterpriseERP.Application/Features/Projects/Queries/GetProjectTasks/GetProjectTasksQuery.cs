using EnterpriseERP.Application.Features.Projects.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Queries.GetProjectTasks;

public class GetProjectTasksQuery : IRequest<List<ProjectTaskDto>>
{
    public Guid ProjectId { get; set; }
}
