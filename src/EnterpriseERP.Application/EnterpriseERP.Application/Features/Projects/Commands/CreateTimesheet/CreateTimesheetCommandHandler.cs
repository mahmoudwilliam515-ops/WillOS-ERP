using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Projects.DTOs;
using EnterpriseERP.Domain.Entities.Projects;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Commands.CreateTimesheet;

public class CreateTimesheetCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateTimesheetCommand, TimesheetDto>
{
    public async Task<TimesheetDto> Handle(CreateTimesheetCommand request, CancellationToken cancellationToken)
    {
        if (request.PeriodEnd.Date < request.PeriodStart.Date)
        {
            throw new ProjectDomainException("Timesheet period end cannot be before period start.");
        }

        if (request.Lines.Count == 0)
        {
            throw new ProjectDomainException("Timesheet must contain at least one line.");
        }

        var duplicate = (await unitOfWork.Repository<Timesheet>().FindAsync(t =>
            t.TimesheetNumber == request.TimesheetNumber)).Any();
        if (duplicate)
        {
            throw new ProjectDomainException("Timesheet number already exists.");
        }

        var timesheet = new Timesheet
        {
            Id = Guid.NewGuid(),
            TimesheetNumber = request.TimesheetNumber,
            EmployeeId = request.EmployeeId,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd
        };

        foreach (var lineRequest in request.Lines)
        {
            if (lineRequest.Hours <= 0 || lineRequest.HourlyRate < 0)
            {
                throw new ProjectDomainException("Timesheet line hours must be positive and rate cannot be negative.");
            }

            var task = await unitOfWork.Repository<ProjectTask>().GetByIdAsync(lineRequest.ProjectTaskId);
            if (task == null || task.ProjectId != lineRequest.ProjectId)
            {
                throw new ProjectDomainException("Timesheet line task must belong to the selected project.");
            }

            if (lineRequest.WorkDate.Date < request.PeriodStart.Date || lineRequest.WorkDate.Date > request.PeriodEnd.Date)
            {
                throw new ProjectDomainException("Timesheet work date must be inside the timesheet period.");
            }

            timesheet.Lines.Add(new TimesheetLine
            {
                Id = Guid.NewGuid(),
                TimesheetId = timesheet.Id,
                ProjectId = lineRequest.ProjectId,
                ProjectTaskId = lineRequest.ProjectTaskId,
                WorkDate = lineRequest.WorkDate,
                Hours = lineRequest.Hours,
                HourlyRate = lineRequest.HourlyRate,
                Description = lineRequest.Description
            });
        }

        timesheet.RecalculateTotals();
        await unitOfWork.Repository<Timesheet>().AddAsync(timesheet);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjectMapper.ToDto(timesheet);
    }
}
