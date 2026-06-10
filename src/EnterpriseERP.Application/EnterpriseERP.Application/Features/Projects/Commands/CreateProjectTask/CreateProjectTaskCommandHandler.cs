using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Projects.DTOs;
using EnterpriseERP.Domain.Entities.Projects;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Commands.CreateProjectTask;

public class CreateProjectTaskCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateProjectTaskCommand, ProjectTaskDto>
{
    public async Task<ProjectTaskDto> Handle(CreateProjectTaskCommand request, CancellationToken cancellationToken)
    {
        var project = await unitOfWork.Repository<Project>().GetByIdAsync(request.ProjectId);
        if (project == null)
        {
            throw new ProjectDomainException("Project not found.");
        }

        if (request.ParentTaskId.HasValue)
        {
            var parent = await unitOfWork.Repository<ProjectTask>().GetByIdAsync(request.ParentTaskId.Value);
            if (parent == null || parent.ProjectId != request.ProjectId)
            {
                throw new ProjectDomainException("Parent WBS task must belong to the same project.");
            }
        }

        var duplicate = (await unitOfWork.Repository<ProjectTask>().FindAsync(t =>
            t.ProjectId == request.ProjectId && t.WbsCode == request.WbsCode)).Any();
        if (duplicate)
        {
            throw new ProjectDomainException("WBS code already exists for this project.");
        }

        var task = new ProjectTask
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ParentTaskId = request.ParentTaskId,
            WbsCode = request.WbsCode,
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            PlannedHours = request.PlannedHours
        };

        await unitOfWork.Repository<ProjectTask>().AddAsync(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjectMapper.ToDto(task);
    }
}
