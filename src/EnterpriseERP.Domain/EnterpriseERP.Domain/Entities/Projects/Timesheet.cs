using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Projects;

public enum TimesheetStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

public class Timesheet : AuditableEntity, IAggregateRoot
{
    public string TimesheetNumber { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;
    public decimal TotalHours { get; set; }
    public decimal TotalLaborCost { get; set; }
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
    public string ApprovedBy { get; set; } = string.Empty;
    public DateTime? ApprovedAt { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public ICollection<TimesheetLine> Lines { get; set; } = new List<TimesheetLine>();

    public void Submit(string submittedBy)
    {
        if (Status != TimesheetStatus.Draft)
        {
            throw new ProjectDomainException("Only draft timesheets can be submitted.");
        }

        if (!Lines.Any())
        {
            throw new ProjectDomainException("Timesheet must contain at least one line.");
        }

        Status = TimesheetStatus.Submitted;
        SubmittedBy = submittedBy;
        SubmittedAt = DateTime.UtcNow;
        RecalculateTotals();
    }

    public void Approve(string approvedBy)
    {
        if (Status != TimesheetStatus.Submitted)
        {
            throw new ProjectDomainException("Only submitted timesheets can be approved.");
        }

        Status = TimesheetStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        RecalculateTotals();
    }

    public void Reject(string rejectedBy, string reason)
    {
        if (Status != TimesheetStatus.Submitted)
        {
            throw new ProjectDomainException("Only submitted timesheets can be rejected.");
        }

        Status = TimesheetStatus.Rejected;
        ApprovedBy = rejectedBy;
        ApprovedAt = DateTime.UtcNow;
        RejectionReason = reason;
    }

    public void RecalculateTotals()
    {
        TotalHours = Lines.Sum(x => x.Hours);
        TotalLaborCost = Lines.Sum(x => x.LaborCost);
    }
}
