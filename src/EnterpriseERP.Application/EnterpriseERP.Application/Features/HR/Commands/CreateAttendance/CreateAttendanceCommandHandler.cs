using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.HR;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.HR.Commands.CreateAttendance;

public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateAttendanceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
    {
        var attendance = new Attendance
        {
            EmployeeId = request.EmployeeId,
            Date = request.Date.Date,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            Status = (AttendanceStatus)request.Status
        };

        await _unitOfWork.Repository<Attendance>().AddAsync(attendance);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(attendance.Id);
    }
}
