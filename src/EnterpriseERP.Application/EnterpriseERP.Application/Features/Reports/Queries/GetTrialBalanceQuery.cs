using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Reports.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;

namespace EnterpriseERP.Application.Features.Reports.Queries;

public record GetTrialBalanceQuery(DateTime AsOfDate) : IRequest<TrialBalanceReportDto>;

public class GetTrialBalanceQueryHandler : IRequestHandler<GetTrialBalanceQuery, TrialBalanceReportDto>
{
    private readonly IGenericRepository<JournalEntry> _journalRepo;

    public GetTrialBalanceQueryHandler(IGenericRepository<JournalEntry> journalRepo)
    {
        _journalRepo = journalRepo;
    }

    public async Task<TrialBalanceReportDto> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        var allEntries = await _journalRepo.GetAllAsync();
        var entries = allEntries
            .Where(e => e.Status == JournalEntryStatus.Posted && e.EntryDate <= request.AsOfDate)
            .ToList();

        var allLines = entries.SelectMany(e => e.Lines).ToList();

        var lines = allLines
            .GroupBy(l => new { l.AccountCode, l.AccountName })
            .Select(g => new TrialBalanceLine
            {
                AccountCode  = g.Key.AccountCode,
                AccountName  = g.Key.AccountName,
                TotalDebit   = g.Sum(l => l.DebitAmount),
                TotalCredit  = g.Sum(l => l.CreditAmount),
                Balance      = g.Sum(l => l.DebitAmount) - g.Sum(l => l.CreditAmount)
            })
            .OrderBy(l => l.AccountCode)
            .ToList();

        return new TrialBalanceReportDto
        {
            AsOfDate     = request.AsOfDate,
            Lines        = lines,
            TotalDebit   = lines.Sum(l => l.TotalDebit),
            TotalCredit  = lines.Sum(l => l.TotalCredit)
        };
    }
}
