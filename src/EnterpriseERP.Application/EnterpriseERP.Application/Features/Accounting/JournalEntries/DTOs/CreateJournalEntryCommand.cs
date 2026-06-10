using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.JournalEntries.DTOs;

public class CreateJournalEntryCommand : IRequest<JournalEntryDto>
{
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public string ReferenceType { get; set; } = "Manual";
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "EGP";
    public decimal ExchangeRate { get; set; } = 1.0m;
    public List<JournalEntryLineRequest> Lines { get; set; } = new();
}
