using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.HR;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Services;

public class PeriodClosingService : IPeriodClosingService
{
    private readonly IUnitOfWork _unitOfWork;

    public PeriodClosingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsOpenAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var periods = await _unitOfWork.Repository<AccountingPeriod>()
            .FindAsync(p => p.StartDate.Date <= date.Date && p.EndDate.Date >= date.Date);
        var period = periods.FirstOrDefault();
        return period != null && period.Status == AccountingPeriodStatus.Open;
    }

    public async Task ValidateOpenAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var periods = await _unitOfWork.Repository<AccountingPeriod>()
            .FindAsync(p => p.StartDate.Date <= date.Date && p.EndDate.Date >= date.Date);
        var period = periods.FirstOrDefault();

        if (period == null)
            throw new AccountingDomainException($"No accounting period found for date {date:yyyy-MM-dd}.");

        if (period.Status == AccountingPeriodStatus.Closed)
            throw new AccountingDomainException($"The accounting period '{period.PeriodName}' is closed.");
    }

    public async Task<bool> ValidateChecklistAsync(Guid periodId, CancellationToken cancellationToken = default)
    {
        var period = await _unitOfWork.Repository<AccountingPeriod>().GetByIdAsync(periodId);
        if (period == null) return false;

        var start = period.StartDate.Date;
        var end = period.EndDate.Date;

        // 1. Unposted journal entries
        var unpostedEntries = await _unitOfWork.Repository<JournalEntry>()
            .FindAsync(e => e.EntryDate >= start && e.EntryDate <= end && e.Status == JournalEntryStatus.Draft);

        // 2. Unmatched / on-hold purchase invoices (AP / P2P)
        var unmatchedPI = await _unitOfWork.Repository<PurchaseInvoice>()
            .FindAsync(i => i.InvoiceDate >= start && i.InvoiceDate <= end &&
                            (i.Status == PurchaseInvoiceStatus.OnHold || i.Status == PurchaseInvoiceStatus.Draft));

        // 3. Deliveries without sales invoice (AR / O2C)
        var pendingDN = await _unitOfWork.Repository<DeliveryNote>()
            .FindAsync(d => d.DeliveryDate >= start && d.DeliveryDate <= end && !d.SalesInvoiceId.HasValue);

        // 4. Open cycle counts in period
        var openCounts = await _unitOfWork.Repository<CycleCount>()
            .FindAsync(c => c.CountDate >= start && c.CountDate <= end && c.Status != CycleCountStatus.Completed);

        // 5. Depreciation posted in period for active assets
        var activeAssets = await _unitOfWork.Repository<FixedAsset>()
            .FindAsync(a => a.IsActive);
        var depreciationLogs = await _unitOfWork.Repository<AssetDepreciationLog>()
            .FindAsync(l => l.PostingDate >= start && l.PostingDate <= end);
        var assetsNeedingDepreciation = activeAssets.Any(a =>
            !(depreciationLogs.Any(l => l.FixedAssetId == a.Id) ||
              (a.LastDepreciationDate.HasValue && a.LastDepreciationDate.Value.Date >= start && a.LastDepreciationDate.Value.Date <= end)));

        // 6. Payroll processed for months overlapping the period
        var periodMonths = Enumerable.Range(0, (end.Year - start.Year) * 12 + end.Month - start.Month + 1)
            .Select(m => start.AddMonths(m))
            .Select(d => (d.Year, d.Month))
            .Distinct()
            .ToList();

        var payrollRuns = await _unitOfWork.Repository<PayrollRun>().GetAllAsync();
        var payrollOk = !periodMonths.Any() || periodMonths.All(pm =>
            payrollRuns.Any(r => r.Year == pm.Year && r.Month == pm.Month &&
                                 r.Status >= PayrollStatus.Processed));

        // 7. Bank reconciliations closed with statement date in period
        var bankReconsInPeriod = await _unitOfWork.Repository<EnterpriseERP.Domain.Entities.Treasury.BankReconciliation>()
            .FindAsync(b => b.StatementDate >= start && b.StatementDate <= end);

        period.IsAPReconciled = !unpostedEntries.Any() && !unmatchedPI.Any();
        period.IsARReconciled = !pendingDN.Any();
        period.IsInventoryReconciled = !openCounts.Any();
        period.IsFixedAssetsDepreciated = !assetsNeedingDepreciation;
        period.IsPayrollPosted = payrollOk;
        period.IsBankReconciled = !bankReconsInPeriod.Any() || bankReconsInPeriod.All(b => b.IsClosed);

        _unitOfWork.Repository<AccountingPeriod>().Update(period);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return period.IsAPReconciled && period.IsARReconciled && period.IsInventoryReconciled
               && period.IsFixedAssetsDepreciated && period.IsPayrollPosted && period.IsBankReconciled;
    }

    public async Task<AccountingPeriod> ClosePeriodAsync(Guid periodId, string closedBy, CancellationToken cancellationToken = default)
    {
        var period = await _unitOfWork.Repository<AccountingPeriod>().GetByIdAsync(periodId);
        if (period == null) throw new AccountingDomainException("Period not found.");
        if (period.Status == AccountingPeriodStatus.Closed) throw new AccountingDomainException("Period is already closed.");

        var isReady = await ValidateChecklistAsync(periodId, cancellationToken);
        if (!isReady)
            throw new AccountingDomainException(
                "Period cannot be closed. Complete all checklist items: AP, AR, Inventory, Assets, Payroll, and Bank reconciliation.");

        period.Status = AccountingPeriodStatus.Closed;
        period.ClosedBy = closedBy;
        period.ClosedAt = DateTime.UtcNow;

        _unitOfWork.Repository<AccountingPeriod>().Update(period);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return period;
    }
}
