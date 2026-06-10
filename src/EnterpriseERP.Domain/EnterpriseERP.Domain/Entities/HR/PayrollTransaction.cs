using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.HR;

public class PayrollTransaction : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public DateTime PayrollPeriodStart { get; set; }
    public DateTime PayrollPeriodEnd { get; set; }
    public DateTime PaymentDate { get; set; }

    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetSalary { get; set; }

    public bool IsProcessed { get; set; } = false;

    // Link to double entry accounting
    public Guid? JournalEntryId { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

