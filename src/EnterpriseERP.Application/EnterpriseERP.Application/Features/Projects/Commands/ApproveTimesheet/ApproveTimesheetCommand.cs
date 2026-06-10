using EnterpriseERP.Application.Features.Projects.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Projects.Commands.ApproveTimesheet;

public class ApproveTimesheetCommand : IRequest<TimesheetDto>
{
    public Guid TimesheetId { get; set; }
    public string ApprovedBy { get; set; } = string.Empty;
}
