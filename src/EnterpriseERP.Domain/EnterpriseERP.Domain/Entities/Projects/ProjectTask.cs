using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Projects;

public enum ProjectTaskStatus
{
    Planned = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public class ProjectTask : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid? ParentTaskId { get; set; }
    public ProjectTask? ParentTask { get; set; }
    public ICollection<ProjectTask> SubTasks { get; set; } = new List<ProjectTask>();
    public string WbsCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.Planned;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal PlannedHours { get; set; }
    public decimal ActualHours { get; set; }
    public decimal BudgetAmount { get; set; }
    public decimal ActualLaborCost { get; set; }
    public decimal ActualMaterialCost { get; set; }
    public decimal ActualExpenseCost { get; set; }
    public decimal CommitmentAmount { get; set; }
    public decimal TotalActualCost => ActualLaborCost + ActualMaterialCost + ActualExpenseCost;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public void AddActuals(decimal hours, decimal laborCost, string updatedBy)
    {
        if (hours <= 0)
        {
            throw new ProjectDomainException("Timesheet hours must be greater than zero.");
        }

        if (laborCost < 0)
        {
            throw new ProjectDomainException("Labor cost cannot be negative.");
        }

        ActualHours += hours;
        ActualLaborCost += laborCost;
        Status = Status == ProjectTaskStatus.Planned ? ProjectTaskStatus.InProgress : Status;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
