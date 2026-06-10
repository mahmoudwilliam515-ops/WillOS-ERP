using EnterpriseERP.SharedKernel.Common;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.HR;

public class Department : AuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public Guid? ManagerId { get; set; }
}

public class JobPosition : AuditableEntity, IAggregateRoot
{
    public string Title { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
}

public class EmployeeGrade : AuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty; // e.g., Grade 1, Senior, Manager
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
}

public enum ContractStatus
{
    Active = 0,
    Expired = 1,
    Terminated = 2,
    Draft = 3
}

public class EmployeeContract : AuditableEntity, IAggregateRoot
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportationAllowance { get; set; }
    public decimal OtherAllowances { get; set; }

    public bool IsGOSIApplicable { get; set; } // التأمينات الاجتماعية
    public decimal EmployeeGOSIPercent { get; set; }
    public decimal CompanyGOSIPercent { get; set; }

    public ContractStatus Status { get; set; } = ContractStatus.Draft;
}
