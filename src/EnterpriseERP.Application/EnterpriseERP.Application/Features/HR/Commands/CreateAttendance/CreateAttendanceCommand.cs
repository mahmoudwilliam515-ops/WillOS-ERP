using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.HR.Commands.CreateAttendance;

public class CreateAttendanceCommand : IRequest<Result<Guid>>
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? CheckIn { get; set; }
    public TimeSpan? CheckOut { get; set; }
    public int Status { get; set; } // 0=Present, 1=Late, 2=Absent, 3=Excused
}
