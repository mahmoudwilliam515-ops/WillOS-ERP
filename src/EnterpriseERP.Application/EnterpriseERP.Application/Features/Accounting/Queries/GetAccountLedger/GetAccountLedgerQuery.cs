using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;
using System.Linq.Expressions;

namespace EnterpriseERP.Application.Features.Accounting.Queries.GetAccountLedger;

public record GetAccountLedgerQuery(Guid PartyId, int Page = 1, int PageSize = 100) : IRequest<AccountLedgerResult>;

public class AccountLedgerResult
{
    public Guid PartyId { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Balance { get; set; }
    public List<LedgerEntryDto> Entries { get; set; } = new();
}

public class LedgerEntryDto
{
    public Guid JournalEntryId { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class GetAccountLedgerQueryHandler : IRequestHandler<GetAccountLedgerQuery, AccountLedgerResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAccountLedgerQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountLedgerResult> Handle(GetAccountLedgerQuery request, CancellationToken cancellationToken)
    {
        var lines = (await _unitOfWork.Repository<JournalEntryLine>()
            .FindAsync(l => l.PartyId == request.PartyId)).ToList();

        var jeIds = lines.Select(l => l.JournalEntryId).Distinct().ToList();
        var journalEntries = (await _unitOfWork.Repository<JournalEntry>()
            .FindAsync(je => jeIds.Contains(je.Id))).ToDictionary(je => je.Id);

        var orderedLines = lines.OrderBy(l => journalEntries[l.JournalEntryId].EntryDate).ToList();

        var result = new AccountLedgerResult
        {
            PartyId = request.PartyId
        };

        decimal runningBalance = 0;

        foreach (var line in orderedLines)
        {
            var je = journalEntries[line.JournalEntryId];
            runningBalance += (line.DebitAmount - line.CreditAmount);

            result.Entries.Add(new LedgerEntryDto
            {
                JournalEntryId = line.JournalEntryId,
                EntryNumber = je.EntryNumber,
                Date = je.EntryDate,
                Debit = line.DebitAmount,
                Credit = line.CreditAmount,
                Description = je.Description,
                RunningBalance = runningBalance
            });
        }

        result.TotalDebit = orderedLines.Sum(l => l.DebitAmount);
        result.TotalCredit = orderedLines.Sum(l => l.CreditAmount);
        result.Balance = runningBalance;

        // Pagination (in-memory for now)
        result.Entries = result.Entries
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return result;
    }
}
