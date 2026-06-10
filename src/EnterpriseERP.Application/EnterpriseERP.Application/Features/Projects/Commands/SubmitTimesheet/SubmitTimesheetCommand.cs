using EnterpriseERP.Application.Features.Projects.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Commands.SubmitTimesheet;

public class SubmitTimesheetCommand : IRequest<TimesheetDto>
{
    public Guid TimesheetId { get; set; }
    public string SubmittedBy { get; set; } = string.Empty;
}
