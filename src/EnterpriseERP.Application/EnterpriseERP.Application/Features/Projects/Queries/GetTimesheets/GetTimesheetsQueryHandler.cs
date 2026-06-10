using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Projects.DTOs;
using EnterpriseERP.Domain.Entities.Projects;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Queries.GetTimesheets;

public class GetTimesheetsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTimesheetsQuery, List<TimesheetDto>>
{
    public async Task<List<TimesheetDto>> Handle(GetTimesheetsQuery request, CancellationToken cancellationToken)
    {
        var timesheets = (await unitOfWork.Repository<Timesheet>().FindAsync(t =>
            (!request.Status.HasValue || t.Status == request.Status.Value) &&
            (string.IsNullOrWhiteSpace(request.EmployeeId) || t.EmployeeId == request.EmployeeId)))
            .OrderByDescending(t => t.PeriodStart)
            .ToList();

        foreach (var timesheet in timesheets)
        {
            timesheet.Lines = (await unitOfWork.Repository<TimesheetLine>().FindAsync(l =>
                l.TimesheetId == timesheet.Id)).ToList();
        }

        return timesheets.Select(ProjectMapper.ToDto).ToList();
    }
}
