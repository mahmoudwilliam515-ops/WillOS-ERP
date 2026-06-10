namespace EnterpriseERP.Application.Features.HR.DTOs;

public class PayrollTransactionDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime PayrollPeriodStart { get; set; }
    public DateTime PayrollPeriodEnd { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetSalary { get; set; }
    public Guid? JournalEntryId { get; set; }
}
