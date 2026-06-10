using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using FixedAsset = EnterpriseERP.Domain.Entities.FixedAssets.FixedAsset;
using EnterpriseERP.Domain.Entities.FixedAssets;
using EnterpriseERP.Domain.Entities.Accounting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Services;

public interface IFixedAssetService
{
    Task<int> ProcessMonthlyDepreciationAsync(DateTime runDate);
}

public class FixedAssetService : IFixedAssetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountingPostingService _accountingPostingService;

    public FixedAssetService(IUnitOfWork unitOfWork, IAccountingPostingService accountingPostingService)
    {
        _unitOfWork = unitOfWork;
        _accountingPostingService = accountingPostingService;
    }

    public async Task<int> ProcessMonthlyDepreciationAsync(DateTime runDate)
    {
        var assets = await _unitOfWork.Repository<FixedAsset>().FindAsync(a => a.IsActive && a.NetBookValue > a.SalvageValue);
        int processedCount = 0;

        foreach (var asset in assets)
        {
            // Simplified depreciation logic for Sprint 1
            decimal annualDepreciation = (asset.PurchaseCost - asset.SalvageValue) / Math.Max(1, asset.UsefulLifeYears);
            decimal monthlyDepreciation = annualDepreciation / 12;

            if (monthlyDepreciation > 0)
            {
                // Create Journal Entry via Posting Service
                var journalEntry = await _accountingPostingService.PostDepreciationAsync(asset, monthlyDepreciation, runDate, default);
                
                asset.AccumulatedDepreciation += monthlyDepreciation;
                asset.NetBookValue = asset.PurchaseCost - asset.AccumulatedDepreciation;
                
                // Add transaction log
                asset.DepreciationTransactions.Add(new DepreciationTransaction
                {
                    Id = Guid.NewGuid(),
                    DepreciationDate = runDate,
                    Amount = monthlyDepreciation,
                    Notes = $"Monthly Depreciation - {runDate:yyyy-MM}",
                    JournalEntryId = journalEntry.Id
                });

                _unitOfWork.Repository<FixedAsset>().Update(asset);
                processedCount++;
            }
        }

        await _unitOfWork.SaveChangesAsync(default);
        return processedCount;
    }
}
