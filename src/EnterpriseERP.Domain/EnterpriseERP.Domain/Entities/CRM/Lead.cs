using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.CRM;

public enum LeadStatus
{
    New = 0,
    Contacted = 1,
    Qualified = 2,
    Lost = 3,
    Converted = 4
}

public class Lead : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public decimal EstimatedValue { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public class Opportunity : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string Title { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public decimal ExpectedRevenue { get; set; }
    public double ProbabilityPercentage { get; set; }
    public DateTime ExpectedCloseDate { get; set; }
    public string Stage { get; set; } = "Prospecting";

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
