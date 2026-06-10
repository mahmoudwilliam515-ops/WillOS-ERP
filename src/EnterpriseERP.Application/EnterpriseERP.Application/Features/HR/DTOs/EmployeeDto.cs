namespace EnterpriseERP.Application.Features.HR.DTOs;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string JobTitle { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportationAllowance { get; set; }
    public decimal TotalSalary => BasicSalary + HousingAllowance + TransportationAllowance;
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
}
