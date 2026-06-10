using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Projects;

public enum ProjectStatus
{
    Draft = 0,
    Active = 1,
    OnHold = 2,
    Completed = 3,
    Cancelled = 4
}

public class Project : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public decimal BudgetAmount { get; set; }
    public decimal ActualLaborCost { get; set; }
    public decimal ActualMaterialCost { get; set; }
    public decimal ActualExpenseCost { get; set; }
    public decimal CommitmentAmount { get; set; } // POs not yet invoiced
    public decimal TotalActualCost => ActualLaborCost + ActualMaterialCost + ActualExpenseCost;
    public decimal RemainingBudget => BudgetAmount - TotalActualCost - CommitmentAmount;
    public string Description { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

    public void Activate(string updatedBy)
    {
        if (Status == ProjectStatus.Cancelled || Status == ProjectStatus.Completed)
        {
            throw new ProjectDomainException("Cannot activate a completed or cancelled project.");
        }

        Status = ProjectStatus.Active;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddLaborCost(decimal amount, string updatedBy)
    {
        if (amount < 0)
        {
            throw new ProjectDomainException("Labor cost cannot be negative.");
        }

        ActualLaborCost += amount;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
