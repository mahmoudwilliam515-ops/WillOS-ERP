namespace EnterpriseERP.Application.Features.Accounting.JournalEntries.DTOs;

public class JournalEntryLineRequest
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? CostCenterId { get; set; }
    public Guid? PartyId { get; set; }
}
