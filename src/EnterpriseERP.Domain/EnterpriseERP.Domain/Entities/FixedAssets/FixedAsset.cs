using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.FixedAssets;

public enum DepreciationMethod
{
    StraightLine = 1,
    DecliningBalance = 2,
    UnitsOfProduction = 3,
    SumOfTheYearsDigits = 4
}

public class FixedAsset : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string Code { get; set; } = string.Empty;
    public string AssetNumber => Code;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Name => NameAr; // For compatibility
    public string? Description { get; set; }
    
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeYears { get; set; }
    
    // For Units of Production Method
    public decimal TotalEstimatedUnits { get; set; }
    public decimal UnitsProducedSoFar { get; set; }
    
    // IFRS Revaluation Model
    public decimal RevaluationAmount { get; set; }
    public DateTime? LastRevaluationDate { get; set; }
    
    public DepreciationMethod DepreciationMethod { get; set; }
    
    public decimal AccumulatedDepreciation { get; set; }
    public decimal NetBookValue { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public Guid BranchId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? WorkCenterId { get; set; } // Linked to manufacturing work center
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<DepreciationTransaction> DepreciationTransactions { get; set; } = new List<DepreciationTransaction>();

    public void CalculateNetBookValue()
    {
        var baseValue = RevaluationAmount > 0 ? RevaluationAmount : PurchaseCost;
        NetBookValue = baseValue - AccumulatedDepreciation;
    }

    public decimal CalculateDepreciation(decimal unitsProducedInPeriod = 0)
    {
        if (UsefulLifeYears <= 0) return 0;

        var baseCost = RevaluationAmount > 0 ? RevaluationAmount : PurchaseCost;
        var depreciableAmount = baseCost - SalvageValue;

        return DepreciationMethod switch
        {
            DepreciationMethod.StraightLine => depreciableAmount / (UsefulLifeYears * 12),
            
            DepreciationMethod.DecliningBalance => (NetBookValue * 2.0m) / (UsefulLifeYears * 12),
            
            DepreciationMethod.UnitsOfProduction => TotalEstimatedUnits > 0 
                ? (depreciableAmount / TotalEstimatedUnits) * unitsProducedInPeriod 
                : 0,
            
            DepreciationMethod.SumOfTheYearsDigits => CalculateSYDDepreciation(depreciableAmount),
            
            _ => 0
        };
    }

    private decimal CalculateSYDDepreciation(decimal depreciableAmount)
    {
        // SYD = (Remaining Life / Sum of Years) * Depreciable Amount
        // Sum of Years = n(n+1)/2
        decimal n = UsefulLifeYears;
        decimal sumOfYears = n * (n + 1) / 2;
        
        // Calculate which year we are in
        int yearOfLife = (int)((DateTime.UtcNow - PurchaseDate).TotalDays / 365) + 1;
        if (yearOfLife > UsefulLifeYears) return 0;
        
        decimal remainingLife = n - yearOfLife + 1;
        
        // Annual depreciation divided by 12 for monthly
        return (remainingLife / sumOfYears) * depreciableAmount / 12;
    }

    public void Revaluate(decimal newAmount, string updatedBy)
    {
        RevaluationAmount = newAmount;
        LastRevaluationDate = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
        
        CalculateNetBookValue();
    }
}

