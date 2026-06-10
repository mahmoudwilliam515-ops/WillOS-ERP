using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Accounting.JournalEntries.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.JournalEntries.Queries.GetAllJournalEntries;

public class GetAllJournalEntriesQueryHandler : IRequestHandler<GetAllJournalEntriesQuery, IEnumerable<JournalEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllJournalEntriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<JournalEntryDto>> Handle(GetAllJournalEntriesQuery request, CancellationToken cancellationToken)
    {
        var entries = await _unitOfWork.Repository<JournalEntry>().GetAllAsync();

        return entries.Select(e => new JournalEntryDto
        {
            Id = e.Id,
            EntryNumber = e.EntryNumber,
            EntryDate = e.EntryDate,
            Description = e.Description,
            ReferenceType = e.ReferenceType,
            ReferenceNumber = e.ReferenceNumber,
            TotalDebit = e.TotalDebit,
            TotalCredit = e.TotalCredit,
            Status = e.Status.ToString(),
            Lines = new List<JournalEntryLineDto>()
        });
    }
}
