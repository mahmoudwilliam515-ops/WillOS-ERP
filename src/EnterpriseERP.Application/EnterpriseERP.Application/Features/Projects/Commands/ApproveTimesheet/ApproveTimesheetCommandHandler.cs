using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Projects.DTOs;
using EnterpriseERP.Domain.Entities.Projects;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Commands.ApproveTimesheet;

public class ApproveTimesheetCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ApproveTimesheetCommand, TimesheetDto>
{
    public async Task<TimesheetDto> Handle(ApproveTimesheetCommand request, CancellationToken cancellationToken)
    {
        var timesheet = await unitOfWork.Repository<Timesheet>().GetByIdAsync(request.TimesheetId);
        if (timesheet == null)
        {
            throw new ProjectDomainException("Timesheet not found.");
        }

        timesheet.Lines = (await unitOfWork.Repository<TimesheetLine>().FindAsync(l =>
            l.TimesheetId == timesheet.Id)).ToList();
        timesheet.Approve(request.ApprovedBy);

        foreach (var line in timesheet.Lines)
        {
            var project = await unitOfWork.Repository<Project>().GetByIdAsync(line.ProjectId);
            var task = await unitOfWork.Repository<ProjectTask>().GetByIdAsync(line.ProjectTaskId);
            if (project == null || task == null)
            {
                throw new ProjectDomainException("Timesheet line references a missing project or task.");
            }

            task.AddActuals(line.Hours, line.LaborCost, request.ApprovedBy);
            project.AddLaborCost(line.LaborCost, request.ApprovedBy);
            unitOfWork.Repository<ProjectTask>().Update(task);
            unitOfWork.Repository<Project>().Update(project);
        }

        unitOfWork.Repository<Timesheet>().Update(timesheet);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjectMapper.ToDto(timesheet);
    }
}
