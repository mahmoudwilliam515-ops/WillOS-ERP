using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Projects.DTOs;
using EnterpriseERP.Domain.Entities.Projects;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Queries.GetProjectTasks;

public class GetProjectTasksQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetProjectTasksQuery, List<ProjectTaskDto>>
{
    public async Task<List<ProjectTaskDto>> Handle(GetProjectTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await unitOfWork.Repository<ProjectTask>().FindAsync(t =>
            !t.IsDeleted && t.ProjectId == request.ProjectId);

        return tasks
            .OrderBy(t => t.WbsCode)
            .Select(ProjectMapper.ToDto)
            .ToList();
    }
}
