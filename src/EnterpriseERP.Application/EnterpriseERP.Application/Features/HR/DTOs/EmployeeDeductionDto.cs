namespace EnterpriseERP.Application.Features.HR.DTOs;

public class EmployeeDeductionDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DeductionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public bool IsApplied { get; set; }
}
