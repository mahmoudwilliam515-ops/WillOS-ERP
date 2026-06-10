using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Taxation;

public class TaxRule : BaseEntity
{
    public Guid TaxCategoryId { get; private set; }
    public string RuleName { get; private set; } = string.Empty;
    public decimal Rate { get; private set; }
    
    /// <summary>
    /// Type of tax, e.g., VAT, WHT (Withholding Tax), EXEMPT
    /// </summary>
    public string TaxType { get; private set; } = "VAT";

    /// <summary>
    /// Applicable Region or Country, e.g., SA, EG, AE
    /// </summary>
    public string Region { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }

    private TaxRule() { } // EF Core

    public TaxRule(Guid taxCategoryId, string ruleName, decimal rate, string taxType, string region, DateTime validFrom)
    {
        TaxCategoryId = taxCategoryId;
        RuleName = ruleName;
        Rate = rate;
        TaxType = taxType;
        Region = region;
        ValidFrom = validFrom;
    }
}
