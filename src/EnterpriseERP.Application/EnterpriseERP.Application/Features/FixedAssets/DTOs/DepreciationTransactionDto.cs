namespace EnterpriseERP.Application.Features.FixedAssets.DTOs;

public class DepreciationTransactionDto
{
    public Guid Id { get; set; }
    public Guid FixedAssetId { get; set; }
    
    public DateTime DepreciationDate { get; set; }
    public decimal Amount { get; set; }
    
    public string? Notes { get; set; }
    public Guid? JournalEntryId { get; set; }
}
