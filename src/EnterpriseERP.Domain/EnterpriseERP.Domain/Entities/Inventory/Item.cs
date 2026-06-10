using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Inventory;

public class Item : AuditableEntity, ISoftDelete
{
    public string Code { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public decimal BuyPrice { get; set; } = 0;
    public decimal LastBuyPrice { get; set; } = 0;
    public decimal AverageCost { get; set; } = 0;
    public int MinStock { get; set; } = 0;
    public int MaxStock { get; set; } = 0;
    public int ReorderPoint { get; set; } = 0;
    public int LeadTimeDays { get; set; } = 0; // Days to procure/produce
    public bool TrackSerial { get; set; } = false;
    public bool HasExpiry { get; set; } = false;
    public bool IsService { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsQualityControlRequired { get; set; } = false; // Added for QC Activation
    public ItemValuationMethod ValuationMethod { get; set; } = ItemValuationMethod.WeightedAverage;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
