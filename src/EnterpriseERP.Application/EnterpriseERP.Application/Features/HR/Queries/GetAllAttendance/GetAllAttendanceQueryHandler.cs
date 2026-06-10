using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.HR;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.HR.Queries.GetAllAttendance;

public class GetAllAttendanceQueryHandler : IRequestHandler<GetAllAttendanceQuery, PagedResult<AttendanceDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllAttendanceQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<AttendanceDto>> Handle(GetAllAttendanceQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Repository<Attendance>()
            .GetPagedAsync(request.PageNumber, request.PageSize);

        var dtos = items.Select(a => new AttendanceDto
        {
            Id = a.Id,
            EmployeeName = a.EmployeeId.ToString(),
            Date = a.Date.ToString("yyyy-MM-dd"),
            CheckIn = a.CheckIn.HasValue ? a.CheckIn.Value.ToString(@"hh\:mm") : "-",
            CheckOut = a.CheckOut.HasValue ? a.CheckOut.Value.ToString(@"hh\:mm") : "-",
            Status = a.Status.ToString()
        });

        return new PagedResult<AttendanceDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
