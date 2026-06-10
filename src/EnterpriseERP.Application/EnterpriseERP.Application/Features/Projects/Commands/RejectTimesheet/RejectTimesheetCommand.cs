using EnterpriseERP.Application.Features.Projects.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Commands.RejectTimesheet;

public class RejectTimesheetCommand : IRequest<TimesheetDto>
{
    public Guid TimesheetId { get; set; }
    public string RejectedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
