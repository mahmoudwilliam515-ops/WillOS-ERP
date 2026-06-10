namespace EnterpriseERP.Application.Features.Accounting.JournalEntries.DTOs;

public class JournalEntryDto
{
    public Guid Id { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<JournalEntryLineDto> Lines { get; set; } = new();
}

public class JournalEntryLineDto
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal BaseDebitAmount { get; set; }
    public decimal BaseCreditAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? CostCenterId { get; set; }
    public Guid? PartyId { get; set; }
}
