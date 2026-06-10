using EnterpriseERP.Application.Features.Projects.DTOs;
using EnterpriseERP.Domain.Entities.Projects;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Queries.GetProjects;

public class GetProjectsQuery : IRequest<List<ProjectDto>>
{
    public ProjectStatus? Status { get; set; }
}
