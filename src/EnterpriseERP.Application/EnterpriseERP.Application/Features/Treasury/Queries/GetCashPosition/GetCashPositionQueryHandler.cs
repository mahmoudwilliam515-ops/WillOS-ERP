using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Domain.Entities.Treasury;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Treasury.Queries.GetCashPosition;

// ═══════════════════════════════════════════════════════════
// Query
// ═══════════════════════════════════════════════════════════

public record GetCashPositionQuery(Guid CompanyId, DateTime AsOfDate) : IRequest<CashPositionDto>;

// ═══════════════════════════════════════════════════════════
// DTOs
// ═══════════════════════════════════════════════════════════

public record CashPositionDto(
    Guid CompanyId,
    DateTime AsOfDate,
    decimal TotalCashBalance,
    decimal TotalOutstandingPayments,
    decimal TotalOutstandingReceipts,
    decimal NetCashPosition,
    List<BankAccountBalanceDto> BankAccounts,
    List<DailyCashFlowDto> DailyFlow);

public record BankAccountBalanceDto(
    Guid BankAccountId,
    string BankAccountName,
    string AccountNumber,
    string CurrencyCode,
    decimal Balance,
    decimal PendingPayments,
    decimal PendingReceipts);

public record DailyCashFlowDto(
    DateTime Date,
    decimal TotalReceipts,
    decimal TotalPayments,
    decimal NetFlow,
    decimal RunningBalance);

// ═══════════════════════════════════════════════════════════
// Handler
// ═══════════════════════════════════════════════════════════

public class GetCashPositionQueryHandler : IRequestHandler<GetCashPositionQuery, CashPositionDto>
{
    private readonly IAppDbContext _context;

    public GetCashPositionQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CashPositionDto> Handle(GetCashPositionQuery request, CancellationToken cancellationToken)
    {
        // 1. جلب أرصدة الحسابات البنكية
        var bankAccounts = await _context.BankAccounts
            .AsNoTracking()
            .Where(b => b.CompanyId == request.CompanyId && b.IsActive)
            .ToListAsync(cancellationToken);

        var bankBalances = new List<BankAccountBalanceDto>();
        decimal totalBalance = 0;

        foreach (var bank in bankAccounts)
        {
            // حساب الرصيد من الحركات حتى تاريخ معين
            var receiptsTotal = await _context.ReceiptVouchers
                .Where(r => r.CompanyId == request.CompanyId
                         && r.BankAccountId == bank.Id
                         && r.ReceiptDate <= request.AsOfDate)
                .SumAsync(r => r.Amount, cancellationToken);

            var paymentsTotal = await _context.PaymentVouchers
                .Where(p => p.CompanyId == request.CompanyId
                         && p.BankAccountId == bank.Id
                         && p.PaymentDate <= request.AsOfDate)
                .SumAsync(p => p.Amount, cancellationToken);

            var balance = receiptsTotal - paymentsTotal;
            totalBalance += balance;

            // المدفوعات المعلقة (موافق عليها لكن لم تُنفَّذ بعد)
            var pendingPayments = await _context.PaymentVouchers
                .Where(p => p.CompanyId == request.CompanyId
                         && p.BankAccountId == bank.Id
                         && p.Status == VoucherStatus.Draft)
                .SumAsync(p => p.Amount, cancellationToken);

            bankBalances.Add(new BankAccountBalanceDto(
                bank.Id,
                bank.BankName,
                bank.AccountNumber,
                bank.Currency ?? "SAR",
                balance,
                pendingPayments,
                0));
        }

        // 2. حساب التدفق اليومي لآخر 30 يوم
        var fromDate = request.AsOfDate.AddDays(-29);
        var dailyFlow = new List<DailyCashFlowDto>();
        decimal runningBalance = totalBalance;

        // الحصول على الحركات اليومية
        var dailyReceipts = await _context.ReceiptVouchers
            .Where(r => r.CompanyId == request.CompanyId
                     && r.ReceiptDate >= fromDate
                     && r.ReceiptDate <= request.AsOfDate)
            .GroupBy(r => r.ReceiptDate.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(r => r.Amount) })
            .ToListAsync(cancellationToken);

        var dailyPayments = await _context.PaymentVouchers
            .Where(p => p.CompanyId == request.CompanyId
                     && p.PaymentDate >= fromDate
                     && p.PaymentDate <= request.AsOfDate
                     && p.Status == VoucherStatus.Approved)
            .GroupBy(p => p.PaymentDate.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(p => p.Amount) })
            .ToListAsync(cancellationToken);

        for (var date = fromDate; date <= request.AsOfDate; date = date.AddDays(1))
        {
            var dayReceipts = dailyReceipts.FirstOrDefault(d => d.Date == date.Date)?.Total ?? 0;
            var dayPayments = dailyPayments.FirstOrDefault(d => d.Date == date.Date)?.Total ?? 0;
            var netFlow = dayReceipts - dayPayments;

            dailyFlow.Add(new DailyCashFlowDto(date, dayReceipts, dayPayments, netFlow, runningBalance));
        }

        // 3. المدفوعات والمستحصلات المعلقة
        var totalPendingPayments = bankBalances.Sum(b => b.PendingPayments);

        return new CashPositionDto(
            request.CompanyId,
            request.AsOfDate,
            totalBalance,
            totalPendingPayments,
            0,
            totalBalance - totalPendingPayments,
            bankBalances,
            dailyFlow);
    }
}
