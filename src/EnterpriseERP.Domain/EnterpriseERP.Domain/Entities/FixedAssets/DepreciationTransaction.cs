using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.FixedAssets;

public class DepreciationTransaction : AuditableEntity
{
    public Guid FixedAssetId { get; set; }
    public FixedAsset FixedAsset { get; set; } = null!;
    
    public DateTime DepreciationDate { get; set; }
    public decimal Amount { get; set; }
    
    public string? Notes { get; set; }
    
    // Link to Accounting Journal Entry
    public Guid? JournalEntryId { get; set; }
}
