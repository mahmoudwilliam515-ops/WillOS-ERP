using EnterpriseERP.SharedKernel.DomainEvents;

namespace EnterpriseERP.Application.Features.HR.Events;

/// <summary>يُطلق بعد معالجة مسيّر رواتب لشهر معين بنجاح</summary>
public class PayrollProcessedEvent : BaseDomainEvent
{
    public int Year { get; }
    public int Month { get; }
    public int EmployeeCount { get; }
    public decimal TotalNetSalaries { get; }
    public decimal TotalDeductions { get; }

    public PayrollProcessedEvent(int year, int month, int employeeCount, decimal totalNetSalaries, decimal totalDeductions)
    {
        Year = year;
        Month = month;
        EmployeeCount = employeeCount;
        TotalNetSalaries = totalNetSalaries;
        TotalDeductions = totalDeductions;
    }
}
