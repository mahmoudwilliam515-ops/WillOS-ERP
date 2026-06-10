using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Application.Features.HR.DTOs;
using EnterpriseERP.Domain.Entities.HR;
using MediatR;
using System.Linq.Expressions;

namespace EnterpriseERP.Application.Features.HR.Queries.GetEmployees;

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    private readonly IGenericRepository<Employee> _repository;

    public GetEmployeesQueryHandler(IGenericRepository<Employee> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<EmployeeDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<Employee, bool>>? predicate = null;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            predicate = e => e.EmployeeNumber.ToLower().Contains(search) ||
                             e.FirstName.ToLower().Contains(search) ||
                             e.LastName.ToLower().Contains(search);
        }

        var (items, totalCount) = await _repository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            predicate);

        var dtos = items.Select(e => new EmployeeDto
        {
            Id = e.Id,
            EmployeeNumber = e.EmployeeNumber,
            FirstName = e.FirstName,
            LastName = e.LastName,
            JobTitle = e.JobTitle,
            Department = e.Department,
            BasicSalary = e.BasicSalary,
            HousingAllowance = e.HousingAllowance,
            TransportationAllowance = e.TransportationAllowance,
            HireDate = e.HireDate,
            IsActive = e.IsActive
        }).ToList();

        return new PagedResult<EmployeeDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
