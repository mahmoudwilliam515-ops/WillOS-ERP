using EnterpriseERP.SharedKernel.Common;
using System.Text.Json.Serialization;

namespace EnterpriseERP.Domain.Entities.SaaS;

public enum TenantStatus
{
    Active = 0,
    Suspended = 1,
    Trial = 2,
    Expired = 3
}

public class Tenant : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string SubDomain { get; set; } = string.Empty;      // erp-companyX.yourdomain.com
    public string? DatabaseName { get; set; }   // للعزل المستقبلي
    public TenantStatus Status { get; set; } = TenantStatus.Active;   // Active, Suspended, Trial, Expired
    public Guid SubscriptionPlanId { get; set; }
    public DateTime SubscriptionStartDate { get; set; }
    public DateTime SubscriptionEndDate { get; set; }
    public int MaxUsers { get; set; }
    public string DefaultCurrency { get; set; } = "SAR"; // ISO 4217: SAR, EGP, USD
    public string DefaultLanguage { get; set; } = "ar"; // ar, en
    public string TimeZone { get; set; } = "Asia/Riyadh";        // 'Asia/Riyadh'
    public string FiscalYearStart { get; set; } = "01-01"; // '01-01' أو '04-01'
    public string? LogoUrl { get; set; }
    public string? AdminEmail { get; set; }
    public string? ConnectionString { get; set; } // For Hybrid Sharding

    // Navigation
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public string Settings { get; set; } = "{}"; // JSON blob

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public class SubscriptionPlan : AuditableEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public int MaxUsers { get; set; }
    public int MaxStorageGB { get; set; }
    public string Features { get; set; } = "{}"; // JSON blob
    public bool IsPublic { get; set; } = true;
}
