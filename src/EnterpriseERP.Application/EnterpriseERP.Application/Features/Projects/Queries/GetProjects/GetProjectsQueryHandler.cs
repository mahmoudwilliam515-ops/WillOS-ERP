using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Projects.DTOs;
using EnterpriseERP.Domain.Entities.Projects;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await unitOfWork.Repository<Project>().FindAsync(p =>
            !p.IsDeleted && (!request.Status.HasValue || p.Status == request.Status.Value));

        return projects
            .OrderBy(p => p.Code)
            .Select(ProjectMapper.ToDto)
            .ToList();
    }
}
