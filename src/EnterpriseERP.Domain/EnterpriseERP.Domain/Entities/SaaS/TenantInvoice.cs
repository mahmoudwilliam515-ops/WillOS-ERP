using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.SaaS;

public enum TenantInvoiceStatus
{
    Pending = 0,
    Paid = 1,
    Overdue = 2,
    Cancelled = 3
}

public enum PaymentMethod
{
    CreditCard = 0,
    BankTransfer = 1,
    PayPal = 2,
    Stripe = 3,
    PayTabs = 4,
    Hyperpay = 5
}

public class TenantInvoice : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public Guid TenantId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    
    public TenantInvoiceStatus Status { get; set; } = TenantInvoiceStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; }
    
    public DateTime? PaidAt { get; set; }
    public string? TransactionId { get; set; } // External payment gateway transaction ID
    
    public string Notes { get; set; } = string.Empty;
    
    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
