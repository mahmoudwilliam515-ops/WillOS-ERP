using EnterpriseERP.Application.Features.Accounting.JournalEntries.DTOs;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Accounting.JournalEntries.Commands.ReverseJournalEntry;

public class ReverseJournalEntryCommand : IRequest<JournalEntryDto>
{
    public Guid OriginalJournalEntryId { get; set; }
    public DateTime ReversalDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}
