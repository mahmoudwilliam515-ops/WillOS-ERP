using EnterpriseERP.SharedKernel.Common;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Taxation;

public class TaxCategory : AuditableEntity, IAggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private readonly List<TaxRule> _taxRules = new();
    public IReadOnlyCollection<TaxRule> TaxRules => _taxRules.AsReadOnly();

    private TaxCategory() { } // EF Core

    public TaxCategory(string code, string name, string description)
    {
        Code = code;
        Name = name;
        Description = description;
    }

    public void AddTaxRule(TaxRule rule)
    {
        _taxRules.Add(rule);
    }
}
