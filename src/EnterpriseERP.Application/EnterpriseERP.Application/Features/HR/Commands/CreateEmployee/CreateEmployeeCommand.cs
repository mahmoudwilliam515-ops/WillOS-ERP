using MediatR;

namespace EnterpriseERP.Application.Features.HR.Commands.CreateEmployee;

public record CreateEmployeeCommand : IRequest<Guid>
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportationAllowance { get; set; }
    public DateTime HireDate { get; set; }
    public Guid BranchId { get; set; }
}
