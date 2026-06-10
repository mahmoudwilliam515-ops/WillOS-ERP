using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Accounting.JournalEntries.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Accounting.Commands.CloseFiscalYear;

public record CloseFiscalYearCommand(Guid FiscalYearId) : IRequest<bool>;

public class CloseFiscalYearCommandHandler : IRequestHandler<CloseFiscalYearCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public CloseFiscalYearCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<bool> Handle(CloseFiscalYearCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var year = await _unitOfWork.Repository<FiscalYear>().GetByIdAsync(request.FiscalYearId);
            if (year == null || year.Status == EnterpriseERP.Domain.Entities.Accounting.FiscalYearStatus.Closed)
                return false;

            // Find Retained Earnings and Income Summary accounts via Mapping
            var mappings = await _unitOfWork.Repository<AccountMapping>().Query()
                .Include(m => m.Account)
                .Where(m => m.PostingKey == PostingKey.INCOME_SUMMARY || m.PostingKey == PostingKey.RETAINED_EARNINGS)
                .ToListAsync(cancellationToken);

            var incomeSummaryAcc = mappings.FirstOrDefault(m => m.PostingKey == PostingKey.INCOME_SUMMARY)?.Account;
            var retainedEarningsAcc = mappings.FirstOrDefault(m => m.PostingKey == PostingKey.RETAINED_EARNINGS)?.Account;

            if (incomeSummaryAcc == null || retainedEarningsAcc == null)
            {
                throw new InvalidOperationException("System Error: Account mapping for 'IncomeSummary' or 'RetainedEarnings' is missing for this tenant.");
            }

            // 2. Calculate Revenue and Expenses using DB-side aggregation for performance
            var revenues = await _unitOfWork.Repository<JournalEntryLine>().Query()
                .Where(l => l.JournalEntry.EntryDate >= year.StartDate && l.JournalEntry.EntryDate <= year.EndDate && l.JournalEntry.Status == JournalEntryStatus.Posted)
                .Where(l => l.Account!.Type == AccountType.Revenue)
                .SumAsync(l => l.CreditAmount - l.DebitAmount, cancellationToken);

            var expenses = await _unitOfWork.Repository<JournalEntryLine>().Query()
                .Where(l => l.JournalEntry.EntryDate >= year.StartDate && l.JournalEntry.EntryDate <= year.EndDate && l.JournalEntry.Status == JournalEntryStatus.Posted)
                .Where(l => l.Account!.Type == AccountType.Expense)
                .SumAsync(l => l.DebitAmount - l.CreditAmount, cancellationToken);

            var netIncome = revenues - expenses;

            var jeCommand = new CreateJournalEntryCommand
            {
                EntryDate = year.EndDate,
                Description = $"Year-End Closing for {year.Name}",
                ReferenceType = "Closing",
                ReferenceNumber = year.Name,
                Lines = new List<JournalEntryLineRequest>()
            };

            if (netIncome > 0)
            {
                jeCommand.Lines.Add(new JournalEntryLineRequest { AccountCode = incomeSummaryAcc.Code, AccountName = incomeSummaryAcc.Name, DebitAmount = netIncome, CreditAmount = 0, Description = "Close Net Income" });
                jeCommand.Lines.Add(new JournalEntryLineRequest { AccountCode = retainedEarningsAcc.Code, AccountName = retainedEarningsAcc.Name, DebitAmount = 0, CreditAmount = netIncome, Description = "Transfer to Retained Earnings" });
            }
            else if (netIncome < 0)
            {
                jeCommand.Lines.Add(new JournalEntryLineRequest { AccountCode = retainedEarningsAcc.Code, AccountName = retainedEarningsAcc.Name, DebitAmount = Math.Abs(netIncome), CreditAmount = 0, Description = "Transfer to Retained Earnings" });
                jeCommand.Lines.Add(new JournalEntryLineRequest { AccountCode = incomeSummaryAcc.Code, AccountName = incomeSummaryAcc.Name, DebitAmount = 0, CreditAmount = Math.Abs(netIncome), Description = "Close Net Loss" });
            }

            if (jeCommand.Lines.Any())
            {
                await _mediator.Send(jeCommand, cancellationToken);
            }

            // 3. Mark all periods as closed
            var periods = await _unitOfWork.Repository<AccountingPeriod>().FindAsync(p => p.FiscalYearId == year.Id);
            foreach (var period in periods)
            {
                period.Status = EnterpriseERP.Domain.Entities.Accounting.AccountingPeriodStatus.Closed;
                _unitOfWork.Repository<AccountingPeriod>().Update(period);
            }

            year.Status = EnterpriseERP.Domain.Entities.Accounting.FiscalYearStatus.Closed;
            _unitOfWork.Repository<FiscalYear>().Update(year);

            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
