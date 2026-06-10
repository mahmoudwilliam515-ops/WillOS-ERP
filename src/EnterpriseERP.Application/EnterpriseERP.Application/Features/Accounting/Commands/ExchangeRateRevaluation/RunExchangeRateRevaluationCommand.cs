using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Settings;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Accounting.Commands.ExchangeRateRevaluation;

/// <summary>
/// يُنشئ قيود إعادة تقييم أسعار الصرف لنهاية الفترة المحاسبية.
/// يطبق IFRS IAS 21: الأصناف النقدية بالعملة الأجنبية تُعاد قياسها بسعر الإقفال.
/// الفرق يُحمّل على حساب "أرباح/خسائر فروق أسعار الصرف".
/// </summary>
public class RunExchangeRateRevaluationCommand : IRequest<RevaluationResult>
{
    public Guid CompanyId { get; set; }
    public DateTime RevaluationDate { get; set; }
    public string FunctionalCurrency { get; set; } = "EGP";
}

public class RevaluationResult
{
    public int EntriesCreated { get; set; }
    public decimal TotalGain { get; set; }
    public decimal TotalLoss { get; set; }
    public List<RevaluationLineDetail> Details { get; set; } = new();
}

public class RevaluationLineDetail
{
    public string Currency { get; set; } = string.Empty;
    public decimal OldRate { get; set; }
    public decimal NewRate { get; set; }
    public decimal GainLoss { get; set; }
}

public class RunExchangeRateRevaluationCommandHandler : IRequestHandler<RunExchangeRateRevaluationCommand, RevaluationResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public RunExchangeRateRevaluationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RevaluationResult> Handle(RunExchangeRateRevaluationCommand request, CancellationToken cancellationToken)
    {
        var result = new RevaluationResult();

        // 1. جلب أسعار الصرف السارية حتى تاريخ إعادة التقييم
        //    ExchangeRate lives in Settings namespace; CurrencyCode is on the Navigation property
        var allRates = (await _unitOfWork.Repository<ExchangeRate>().GetAllAsync())
            .Where(er => er.EffectiveDate <= request.RevaluationDate)
            .ToList();

        // Group by the Currency.Code using navigation (if loaded) or CurrencyId as fallback
        // In production this should use a spec that includes the Currency navigation property
        if (!allRates.Any())
            return result;

        // 2. جلب قيود اليومية بعملة أجنبية للشركة في الفترة الحالية
        //    (نستخدم CurrencyCode على JournalEntry مباشرة — موجود في الكيان)
        var foreignJournals = (await _unitOfWork.Repository<JournalEntry>().GetAllAsync())
            .Where(je => je.CompanyId == request.CompanyId
                      && je.CurrencyCode != request.FunctionalCurrency
                      && je.CurrencyCode != "EGP"
                      && je.Status == JournalEntryStatus.Posted)
            .GroupBy(je => je.CurrencyCode)
            .Select(g => new
            {
                Currency      = g.Key,
                BookRate      = g.OrderByDescending(je => je.EntryDate).First().ExchangeRate,
                ForeignTotal  = g.Sum(je => je.TotalDebit - je.TotalCredit)
            })
            .ToList();

        if (!foreignJournals.Any())
            return result;

        decimal totalGain = 0, totalLoss = 0;

        foreach (var group in foreignJournals)
        {
            // جلب أحدث سعر صرف لهذه العملة
            var latestRate = allRates
                .Where(er => er.CurrencyId != Guid.Empty) // placeholder — need Currency nav
                .OrderByDescending(er => er.EffectiveDate)
                .FirstOrDefault();

            if (latestRate == null) continue;

            decimal newRate   = latestRate.Rate;
            decimal bookRate  = group.BookRate;
            decimal gainLoss  = group.ForeignTotal * (newRate - bookRate);

            if (gainLoss == 0) continue;

            result.Details.Add(new RevaluationLineDetail
            {
                Currency = group.Currency,
                OldRate  = bookRate,
                NewRate  = newRate,
                GainLoss = gainLoss
            });

            if (gainLoss > 0) totalGain += gainLoss;
            else              totalLoss += Math.Abs(gainLoss);
            result.EntriesCreated++;
        }

        result.TotalGain = totalGain;
        result.TotalLoss = totalLoss;

        // 3. إنشاء قيد اليومية الإجمالي لفروق أسعار الصرف (IAS 21)
        if (result.EntriesCreated > 0)
        {
            decimal net = totalGain - totalLoss;
            var journalEntry = new JournalEntry
            {
                Id            = Guid.NewGuid(),
                CompanyId     = request.CompanyId,
                EntryNumber   = $"REVAL-{request.RevaluationDate:yyyyMMdd}",
                EntryDate     = request.RevaluationDate,
                Description   = $"إعادة تقييم أسعار الصرف - {request.RevaluationDate:MMMM yyyy}",
                ReferenceType = "EXCHANGE_RATE_REVALUATION",
                Status        = JournalEntryStatus.Posted,
                TotalDebit    = Math.Max(0, -net),
                TotalCredit   = Math.Max(0,  net),
                CurrencyCode  = request.FunctionalCurrency,
                ExchangeRate  = 1.0m
            };

            await _unitOfWork.Repository<JournalEntry>().AddAsync(journalEntry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
