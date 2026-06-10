using EnterpriseERP.Application.Features.Projects.DTOs;
using EnterpriseERP.Domain.Entities.Projects;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Queries.GetTimesheets;

public class GetTimesheetsQuery : IRequest<List<TimesheetDto>>
{
    public TimesheetStatus? Status { get; set; }
    public string? EmployeeId { get; set; }
}
