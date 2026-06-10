using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.HR.Queries.GetAllAttendance;

public class GetAllAttendanceQuery : IRequest<PagedResult<AttendanceDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SearchTerm { get; set; }
}

public record AttendanceDto
{
    public Guid Id { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string Date { get; init; } = string.Empty;
    public string CheckIn { get; init; } = string.Empty;
    public string CheckOut { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
