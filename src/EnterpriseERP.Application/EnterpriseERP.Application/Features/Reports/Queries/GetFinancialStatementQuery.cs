using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Reporting;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Reports.Queries;

public class FinancialStatementResponse
{
    public string ReportType { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string PeriodId { get; set; } = string.Empty;
    public string GaapBook { get; set; } = string.Empty;
    public bool IsFinalized { get; set; }
    public DateTime GeneratedAt { get; set; }
    public List<FinancialStatementSectionDto> Sections { get; set; } = new();
}

public class FinancialStatementSectionDto
{
    public string SectionCode { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public List<FinancialStatementLineDto> Lines { get; set; } = new();
    public decimal Subtotal { get; set; }
}

public class FinancialStatementLineDto
{
    public string LineCode { get; set; } = string.Empty;
    public string LineName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ComparativeAmount { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePct { get; set; }
}

public record GetFinancialStatementQuery(Guid CompanyId, Guid PeriodId, Guid? ComparativePeriodId, string GaapBook, string ReportType) : IRequest<FinancialStatementResponse>;

public class GetFinancialStatementQueryHandler : IRequestHandler<GetFinancialStatementQuery, FinancialStatementResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFinancialStatementQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FinancialStatementResponse> Handle(GetFinancialStatementQuery request, CancellationToken cancellationToken)
    {
        // 1. Load template and line definitions
        var template = await _unitOfWork.Repository<ReportTemplate>().Query()
            .Include(t => t.Sections)
                .ThenInclude(s => s.Lines)
            .FirstOrDefaultAsync(t => t.CompanyId == request.CompanyId && 
                                      t.ReportType == request.ReportType && 
                                      t.GaapBook == request.GaapBook &&
                                      t.IsActive, cancellationToken);

        if (template == null)
            throw new Exception($"Report template not found for Type: {request.ReportType}, GAAP: {request.GaapBook}");

        // 2. Check period is closed (warn if open — data may be incomplete)
        var period = await _unitOfWork.Repository<AccountingPeriod>().GetByIdAsync(request.PeriodId);
        if (period == null) throw new Exception("Period not found");
        bool isFinalized = period.Status == AccountingPeriodStatus.Closed; // Assuming Status enum exists

        // 3. Fetch trial balance (simplified for now: from GL directly, but conceptually from BI read model)
        var trialBalanceLines = await _unitOfWork.Repository<JournalEntryLine>().Query()
            .Where(l => l.JournalEntry.CompanyId == request.CompanyId &&
                        l.JournalEntry.EntryDate >= period.StartDate &&
                        l.JournalEntry.EntryDate <= period.EndDate &&
                        l.JournalEntry.Status == JournalEntryStatus.Posted)
            .GroupBy(l => l.AccountCode)
            .Select(g => new
            {
                AccountCode = g.Key,
                NetBalance = g.Sum(l => l.DebitAmount - l.CreditAmount)
            })
            .ToListAsync(cancellationToken);

        var tbDictionary = trialBalanceLines.ToDictionary(k => k.AccountCode, v => v.NetBalance);

        // Comparative period logic (Optional)
        Dictionary<string, decimal> compTbDictionary = new();
        if (request.ComparativePeriodId.HasValue)
        {
            var compPeriod = await _unitOfWork.Repository<AccountingPeriod>().GetByIdAsync(request.ComparativePeriodId.Value);
            if (compPeriod != null)
            {
                var compLines = await _unitOfWork.Repository<JournalEntryLine>().Query()
                    .Where(l => l.JournalEntry.CompanyId == request.CompanyId &&
                                l.JournalEntry.EntryDate >= compPeriod.StartDate &&
                                l.JournalEntry.EntryDate <= compPeriod.EndDate &&
                                l.JournalEntry.Status == JournalEntryStatus.Posted)
                    .GroupBy(l => l.AccountCode)
                    .Select(g => new
                    {
                        AccountCode = g.Key,
                        NetBalance = g.Sum(l => l.DebitAmount - l.CreditAmount)
                    })
                    .ToListAsync(cancellationToken);
                
                compTbDictionary = compLines.ToDictionary(k => k.AccountCode, v => v.NetBalance);
            }
        }

        var response = new FinancialStatementResponse
        {
            ReportType = request.ReportType,
            CompanyId = request.CompanyId.ToString(),
            PeriodId = request.PeriodId.ToString(),
            GaapBook = request.GaapBook,
            IsFinalized = isFinalized,
            GeneratedAt = DateTime.UtcNow
        };

        // 4. Compute amounts
        foreach (var section in template.Sections.OrderBy(s => s.SortOrder))
        {
            var sectionDto = new FinancialStatementSectionDto
            {
                SectionCode = section.SectionCode,
                SectionName = section.SectionName
            };

            decimal sectionTotal = 0;

            foreach (var line in section.Lines.OrderBy(l => l.SortOrder))
            {
                decimal amount = 0;
                decimal compAmount = 0;

                // Match Accounts
                if (!string.IsNullOrEmpty(line.AccountFrom) && !string.IsNullOrEmpty(line.AccountTo))
                {
                    amount = tbDictionary.Where(kvp => string.Compare(kvp.Key, line.AccountFrom) >= 0 && string.Compare(kvp.Key, line.AccountTo) <= 0).Sum(kvp => kvp.Value);
                    compAmount = compTbDictionary.Where(kvp => string.Compare(kvp.Key, line.AccountFrom) >= 0 && string.Compare(kvp.Key, line.AccountTo) <= 0).Sum(kvp => kvp.Value);
                }

                if (section.NormalBalance == "C")
                {
                    amount = -amount;
                    compAmount = -compAmount;
                }

                if (section.Negate)
                {
                    amount = -amount;
                    compAmount = -compAmount;
                }

                sectionTotal += amount;

                sectionDto.Lines.Add(new FinancialStatementLineDto
                {
                    LineCode = line.LineCode,
                    LineName = line.LineName,
                    Amount = amount,
                    ComparativeAmount = compAmount,
                    Variance = amount - compAmount,
                    VariancePct = compAmount != 0 ? ((amount - compAmount) / Math.Abs(compAmount)) * 100 : 0
                });
            }

            sectionDto.Subtotal = sectionTotal;
            response.Sections.Add(sectionDto);
        }

        return response;
    }
}
