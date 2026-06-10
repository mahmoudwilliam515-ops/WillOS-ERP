using EnterpriseERP.Domain.Entities.FixedAssets;
using MediatR;

namespace EnterpriseERP.Application.Features.FixedAssets.Commands.CreateFixedAsset;

public class CreateFixedAssetCommand : IRequest<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public decimal SalvageValue { get; set; }
    public int UsefulLifeYears { get; set; }
    
    public DepreciationMethod DepreciationMethod { get; set; }
    
    public Guid BranchId { get; set; }
}
