using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Projects.DTOs;
using EnterpriseERP.Domain.Entities.Projects;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Commands.RejectTimesheet;

public class RejectTimesheetCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<RejectTimesheetCommand, TimesheetDto>
{
    public async Task<TimesheetDto> Handle(RejectTimesheetCommand request, CancellationToken cancellationToken)
    {
        var timesheet = await unitOfWork.Repository<Timesheet>().GetByIdAsync(request.TimesheetId);
        if (timesheet == null)
        {
            throw new ProjectDomainException("Timesheet not found.");
        }

        timesheet.Lines = (await unitOfWork.Repository<TimesheetLine>().FindAsync(l =>
            l.TimesheetId == timesheet.Id)).ToList();
        timesheet.Reject(request.RejectedBy, request.Reason);
        unitOfWork.Repository<Timesheet>().Update(timesheet);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProjectMapper.ToDto(timesheet);
    }
}
