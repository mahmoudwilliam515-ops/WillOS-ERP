using EnterpriseERP.Domain.Entities.FixedAssets;

namespace EnterpriseERP.Application.Features.FixedAssets.DTOs;

public class FixedAssetDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeYears { get; set; }
    
    public DepreciationMethod DepreciationMethod { get; set; }
    
    public decimal AccumulatedDepreciation { get; set; }
    public decimal NetBookValue { get; set; }
    
    public bool IsActive { get; set; }
    public Guid BranchId { get; set; }
}
