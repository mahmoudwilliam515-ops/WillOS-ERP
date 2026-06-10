using EnterpriseERP.Domain.Common;
using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Reporting;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Purchasing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.PeriodClose.Commands.ReconcileSubledgerToGL;

public record ReconcileSubledgerToGLCommand : IRequest<ReconciliationResult>
{
    public Guid CompanyId { get; init; }
    public Guid PeriodId { get; init; }
    public SubledgerType SubledgerType { get; init; }  // AR or AP
}

public enum SubledgerType { AR, AP }

public class ReconcileSubledgerToGLCommandHandler : IRequestHandler<ReconcileSubledgerToGLCommand, ReconciliationResult>
{
    private readonly IAppDbContext _context;

    public ReconcileSubledgerToGLCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ReconciliationResult> Handle(ReconcileSubledgerToGLCommand command, CancellationToken cancellationToken)
    {
        var period = await _context.AccountingPeriods
            .FirstOrDefaultAsync(p => p.Id == command.PeriodId && p.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException($"Period {command.PeriodId} not found");

        if (command.SubledgerType == SubledgerType.AR)
            return await ReconcileARAsync(command.CompanyId, period, cancellationToken);
        else
            return await ReconcileAPAsync(command.CompanyId, period, cancellationToken);
    }

    private async Task<ReconciliationResult> ReconcileARAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
    {
        // مجموع الفواتير غير المسددة من AR Subledger
        var subledgerBalance = await _context.SalesInvoices
            .Where(i => i.CompanyId == companyId
                     && i.InvoiceDate <= period.EndDate
                     && i.Status != InvoiceStatus.Cancelled)
            .SumAsync(i => i.RemainingAmount, ct);

        // رصيد حساب AR Control في GL
        var glBalance = await _context.GeneralLedgerEntries
            .Where(e => e.CompanyId == companyId
                     && e.AccountCategory == AccountCategory.AccountsReceivable
                     && e.PostingDate <= period.EndDate)
            .SumAsync(e => e.DebitAmount - e.CreditAmount, ct);

        var difference = subledgerBalance - glBalance;

        return new ReconciliationResult
        {
            SubledgerType = SubledgerType.AR,
            SubledgerBalance = subledgerBalance,
            GLBalance = glBalance,
            Difference = difference,
            IsReconciled = Math.Abs(difference) <= 0.01m,
            ReconciledAt = DateTime.UtcNow
        };
    }

    private async Task<ReconciliationResult> ReconcileAPAsync(Guid companyId, AccountingPeriod period, CancellationToken ct)
    {
        var subledgerBalance = await _context.PurchaseInvoices
            .Where(i => i.CompanyId == companyId
                     && i.InvoiceDate <= period.EndDate
                     && i.Status != PurchaseInvoiceStatus.Cancelled)
            .SumAsync(i => i.RemainingAmount, ct);

        var glBalance = await _context.GeneralLedgerEntries
            .Where(e => e.CompanyId == companyId
                     && e.AccountCategory == AccountCategory.AccountsPayable
                     && e.PostingDate <= period.EndDate)
            .SumAsync(e => e.CreditAmount - e.DebitAmount, ct);

        var difference = subledgerBalance - glBalance;

        return new ReconciliationResult
        {
            SubledgerType = SubledgerType.AP,
            SubledgerBalance = subledgerBalance,
            GLBalance = glBalance,
            Difference = difference,
            IsReconciled = Math.Abs(difference) <= 0.01m,
            ReconciledAt = DateTime.UtcNow
        };
    }
}

public class ReconciliationResult
{
    public SubledgerType SubledgerType { get; init; }
    public decimal SubledgerBalance { get; init; }
    public decimal GLBalance { get; init; }
    public decimal Difference { get; init; }
    public bool IsReconciled { get; init; }
    public DateTime ReconciledAt { get; init; }
}
