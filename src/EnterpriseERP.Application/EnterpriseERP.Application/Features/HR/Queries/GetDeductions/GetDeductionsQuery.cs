using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.HR.DTOs;
using EnterpriseERP.Domain.Entities.HR;
using MediatR;

namespace EnterpriseERP.Application.Features.HR.Queries.GetDeductions;

public record GetDeductionsQuery : IRequest<List<EmployeeDeductionDto>>
{
    public Guid? EmployeeId { get; init; }
    public int? Year { get; init; }
    public int? Month { get; init; }
    public bool? IsApplied { get; init; }
}

public class GetDeductionsQueryHandler : IRequestHandler<GetDeductionsQuery, List<EmployeeDeductionDto>>
{
    private readonly IGenericRepository<EmployeeDeduction> _deductionRepo;
    private readonly IGenericRepository<Employee> _employeeRepo;

    public GetDeductionsQueryHandler(
        IGenericRepository<EmployeeDeduction> deductionRepo,
        IGenericRepository<Employee> employeeRepo)
    {
        _deductionRepo = deductionRepo;
        _employeeRepo = employeeRepo;
    }

    public async Task<List<EmployeeDeductionDto>> Handle(GetDeductionsQuery request, CancellationToken cancellationToken)
    {
        var deductions = await _deductionRepo.FindAsync(d =>
            (!request.EmployeeId.HasValue || d.EmployeeId == request.EmployeeId.Value) &&
            (!request.Year.HasValue  || d.Year  == request.Year.Value) &&
            (!request.Month.HasValue || d.Month == request.Month.Value) &&
            (!request.IsApplied.HasValue || d.IsApplied == request.IsApplied.Value));

        var employeeIds = deductions.Select(d => d.EmployeeId).Distinct().ToList();
        var employees = await _employeeRepo.FindAsync(e => employeeIds.Contains(e.Id));
        var employeeMap = employees.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");

        return deductions
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new EmployeeDeductionDto
            {
                Id = d.Id,
                EmployeeId = d.EmployeeId,
                EmployeeName = employeeMap.GetValueOrDefault(d.EmployeeId, "Unknown"),
                DeductionType = d.DeductionType.ToString(),
                Description = d.Description,
                Amount = d.Amount,
                Year = d.Year,
                Month = d.Month,
                IsApplied = d.IsApplied
            })
            .ToList();
    }
}
