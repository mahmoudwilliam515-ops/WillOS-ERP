using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.SharedKernel.Exceptions;
using EnterpriseERP.Domain.Common;
using EnterpriseERP.Domain.Enums;
using EnterpriseERP.Domain.Exceptions;

namespace EnterpriseERP.Domain.Entities.Accounting;

/// <summary>
/// يربط كل حدث محاسبي (PostingKey) بالحساب المحاسبي الصحيح
/// بدلاً من Hardcoded account codes في الكود.
/// كل Tenant/Company لها mapping مستقلة.
/// </summary>
public class AccountMapping : AuditableEntity
{
    /// <summary>مفتاح الحدث المحاسبي — مثل: SALES_REVENUE, AR_RECEIVABLE, COGS</summary>
    public PostingKey PostingKey { get; set; }
    
    // For backward compatibility
    public PostingKey PostingType 
    { 
        get => PostingKey;
        set => PostingKey = value;
    }

    /// <summary>الحساب المحاسبي المرتبط</summary>
    public Guid AccountId { get; set; }

    /// <summary>الشركة — لدعم تعدد شركات في نفس الـ Tenant</summary>
    public Guid CompanyId { get; set; }

    /// <summary>هل هذا الـ Mapping فعّال؟</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>ملاحظة توضيحية (اختياري)</summary>
    public string? Description { get; set; }

    // Navigation
    public Account Account { get; set; } = null!;

    public AccountMapping() { }

    public static AccountMapping Create(
        PostingKey postingKey,
        Guid accountId,
        Guid companyId,
        Guid tenantId,
        string? description = null)
    {
        if (accountId == Guid.Empty)
            throw new AccountingDomainException("AccountId مطلوب للـ AccountMapping");
        if (companyId == Guid.Empty)
            throw new AccountingDomainException("CompanyId مطلوب للـ AccountMapping");
        if (tenantId == Guid.Empty)
            throw new AccountingDomainException("TenantId مطلوب للـ AccountMapping");

        return new AccountMapping
        {
            PostingKey = postingKey,
            AccountId = accountId,
            CompanyId = companyId,
            TenantId = tenantId,
            Description = description,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
