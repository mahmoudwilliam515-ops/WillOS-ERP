using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Application.Features.HR.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.HR.Queries.GetEmployees;

public record GetEmployeesQuery : IRequest<PagedResult<EmployeeDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}
