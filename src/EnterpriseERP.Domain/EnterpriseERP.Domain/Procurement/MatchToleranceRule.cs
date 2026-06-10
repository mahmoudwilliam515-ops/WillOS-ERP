using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Procurement;

/// <summary>
/// قواعد التسامح في المطابقة الثلاثية
/// Blueprint Section 1.1 — Tolerance Rules
/// </summary>
public class MatchToleranceRule : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid? ItemId { get; private set; }          // null = ينطبق على كل الأصناف
    public Guid? SupplierId { get; private set; }      // null = ينطبق على كل الموردين
    public decimal PriceTolerancePercent { get; private set; }    // مثال: 2.5 = 2.5%
    public decimal QuantityTolerancePercent { get; private set; } // مثال: 1.0 = 1.0%
    public bool IsActive { get; private set; } = true;

    private MatchToleranceRule() { }

    public static MatchToleranceRule Create(
        Guid companyId,
        decimal priceTolerancePercent,
        decimal quantityTolerancePercent,
        Guid? itemId = null,
        Guid? supplierId = null)
    {
        if (priceTolerancePercent < 0 || priceTolerancePercent > 100)
            throw new ArgumentException("Price tolerance must be between 0 and 100 percent");

        if (quantityTolerancePercent < 0 || quantityTolerancePercent > 100)
            throw new ArgumentException("Quantity tolerance must be between 0 and 100 percent");

        return new MatchToleranceRule
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ItemId = itemId,
            SupplierId = supplierId,
            PriceTolerancePercent = priceTolerancePercent,
            QuantityTolerancePercent = quantityTolerancePercent
        };
    }
}
