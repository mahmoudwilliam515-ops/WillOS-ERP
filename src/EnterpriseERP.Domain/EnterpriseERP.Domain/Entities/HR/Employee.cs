using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Entities.Settings;
using EnterpriseERP.Domain.Common;

namespace EnterpriseERP.Domain.Entities.HR;

public class Employee : AuditableEntity, ISoftDelete, ICompanyEntity
{
    public Guid CompanyId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Department? DepartmentEntity { get; set; }
    public Guid? JobPositionId { get; set; }
    public JobPosition? JobPosition { get; set; }
    public Guid? GradeId { get; set; }
    public EmployeeGrade? Grade { get; set; }
    
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportationAllowance { get; set; }

    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
