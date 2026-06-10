using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Projects.DTOs;
using EnterpriseERP.Domain.Entities.Projects;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var duplicate = (await unitOfWork.Repository<Project>().FindAsync(p => p.Code == request.Code)).Any();
        if (duplicate)
        {
            throw new ProjectDomainException("Project code already exists.");
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            CustomerName = request.CustomerName,
            CostCenterId = request.CostCenterId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            BudgetAmount = request.BudgetAmount,
            Description = request.Description,
            Status = ProjectStatus.Active
        };

        await unitOfWork.Repository<Project>().AddAsync(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjectMapper.ToDto(project);
    }
}
