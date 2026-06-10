using EnterpriseERP.Application.Features.Accounting.JournalEntries.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.JournalEntries.Queries.GetAllJournalEntries;

public class GetAllJournalEntriesQuery : IRequest<IEnumerable<JournalEntryDto>>
{
}
