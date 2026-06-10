using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Settings;

public class Currency : AuditableEntity, IAggregateRoot
{
    public string Code { get; set; } = string.Empty; // e.g., USD, EGP, EUR
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public bool IsBaseCurrency { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ExchangeRate : AuditableEntity
{
    public Guid CurrencyId { get; set; }
    public decimal Rate { get; set; } // Rate to Base Currency
    public DateTime EffectiveDate { get; set; }
    
    // Navigation
    public Currency Currency { get; set; } = null!;
}
