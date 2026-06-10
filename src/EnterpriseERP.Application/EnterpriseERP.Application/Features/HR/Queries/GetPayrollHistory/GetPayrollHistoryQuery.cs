using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Application.Features.HR.DTOs;
using EnterpriseERP.Domain.Entities.HR;
using MediatR;

namespace EnterpriseERP.Application.Features.HR.Queries.GetPayrollHistory;

public record GetPayrollHistoryQuery : IRequest<PagedResult<PayrollTransactionDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? Year { get; init; }
    public int? Month { get; init; }
}

public class GetPayrollHistoryQueryHandler : IRequestHandler<GetPayrollHistoryQuery, PagedResult<PayrollTransactionDto>>
{
    private readonly IGenericRepository<PayrollTransaction> _payrollRepo;
    private readonly IGenericRepository<Employee> _employeeRepo;

    public GetPayrollHistoryQueryHandler(
        IGenericRepository<PayrollTransaction> payrollRepo,
        IGenericRepository<Employee> employeeRepo)
    {
        _payrollRepo = payrollRepo;
        _employeeRepo = employeeRepo;
    }

    public async Task<PagedResult<PayrollTransactionDto>> Handle(
        GetPayrollHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _payrollRepo.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            pt => (!request.Year.HasValue || pt.PayrollPeriodStart.Year == request.Year.Value) &&
                  (!request.Month.HasValue || pt.PayrollPeriodStart.Month == request.Month.Value));

        var employeeIds = items.Select(p => p.EmployeeId).Distinct().ToList();
        var employees = await _employeeRepo.GetPagedAsync(1, 1000, e => employeeIds.Contains(e.Id));
        var employeeMap = employees.Items.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");

        var dtos = items.Select(pt => new PayrollTransactionDto
        {
            Id = pt.Id,
            EmployeeId = pt.EmployeeId,
            EmployeeName = employeeMap.GetValueOrDefault(pt.EmployeeId, "Unknown"),
            PayrollPeriodStart = pt.PayrollPeriodStart,
            PayrollPeriodEnd = pt.PayrollPeriodEnd,
            PaymentDate = pt.PaymentDate,
            BasicSalary = pt.BasicSalary,
            Allowances = pt.Allowances,
            Deductions = pt.Deductions,
            NetSalary = pt.NetSalary,
            JournalEntryId = pt.JournalEntryId
        }).ToList();

        return new PagedResult<PayrollTransactionDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
