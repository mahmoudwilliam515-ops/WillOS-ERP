using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Common;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.HR;

public enum PayrollStatus
{
    Draft = 0,
    Processed = 1,
    Approved = 2,
    Paid = 3,
    Cancelled = 4
}

public class PayrollRun : AuditableEntity, IAggregateRoot, ICompanyEntity
{
    public Guid CompanyId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime ProcessedDate { get; set; }
    
    public decimal TotalBasicSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetAmount { get; set; }
    
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public List<PayrollEntry> Entries { get; set; } = new();
}

public class PayrollEntry : BaseEntity
{
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public decimal BasicSalary { get; set; }
    public decimal HousingAllowance { get; set; }
    public decimal TransportationAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    
    public decimal OvertimeAmount { get; set; }
    public decimal AbsenceDeduction { get; set; }
    public decimal LoanDeduction { get; set; }
    public decimal GOSIDeduction { get; set; }
    
    public decimal NetSalary { get; set; }
}
