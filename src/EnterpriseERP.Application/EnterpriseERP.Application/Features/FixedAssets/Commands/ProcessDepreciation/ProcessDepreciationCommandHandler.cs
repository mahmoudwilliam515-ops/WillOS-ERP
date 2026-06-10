using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.FixedAssets;
using EnterpriseERP.Domain.Enums;
using FixedAsset = EnterpriseERP.Domain.Entities.FixedAssets.FixedAsset;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.FixedAssets.Commands.ProcessDepreciation;

public class ProcessDepreciationCommandHandler : IRequestHandler<ProcessDepreciationCommand, Guid>
{
    private readonly IGenericRepository<FixedAsset> _fixedAssetRepo;
    private readonly IGenericRepository<DepreciationTransaction> _depreciationRepo;
    private readonly IGenericRepository<JournalEntry> _journalRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessDepreciationCommandHandler(
        IGenericRepository<FixedAsset> fixedAssetRepo,
        IGenericRepository<DepreciationTransaction> depreciationRepo,
        IGenericRepository<JournalEntry> journalRepo,
        IUnitOfWork unitOfWork)
    {
        _fixedAssetRepo = fixedAssetRepo;
        _depreciationRepo = depreciationRepo;
        _journalRepo = journalRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(ProcessDepreciationCommand request, CancellationToken cancellationToken)
    {
        var periodStart = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodStr = $"{request.Year}-{request.Month:D2}";

        // 1. Fetch assets that need depreciation
        var assetsResult = await _fixedAssetRepo.GetPagedAsync(1, 1000, 
            a => a.PurchaseDate <= periodStart && a.IsActive);
        
        var activeAssets = assetsResult.Items.ToList();
        if (!activeAssets.Any())
            throw new FixedAssetsDomainException("No active assets found for depreciation.");

        // 2. Fetch Account Mappings for Depreciation
        var mappings = await _unitOfWork.Repository<AccountMapping>().Query()
            .Include(m => m.Account)
            .Where(m => m.PostingKey == PostingKey.DEPRECIATION_EXPENSE || m.PostingKey == PostingKey.ACCUMULATED_DEPRECIATION)
            .ToListAsync(cancellationToken);

        var expenseAcc = mappings.FirstOrDefault(m => m.PostingKey == PostingKey.DEPRECIATION_EXPENSE)?.Account;
        var accumulatedAcc = mappings.FirstOrDefault(m => m.PostingKey == PostingKey.ACCUMULATED_DEPRECIATION)?.Account;

        if (expenseAcc == null || accumulatedAcc == null)
        {
            throw new FixedAssetsDomainException("Account mapping for 'DepreciationExpense' or 'AccumulatedDepreciation' is missing.");
        }

        var depreciationTransactions = new List<DepreciationTransaction>();
        decimal totalDepreciationAmount = 0;

        // 3. Calculate depreciation for each asset using IFRS methods
        foreach (var asset in activeAssets)
        {
            // For Units of Production, we would need to pass actual units. 
            // For now, we use 0 or implement a way to pass this data.
            var monthlyDepreciation = asset.CalculateDepreciation(0);
            if (monthlyDepreciation <= 0) continue;

            totalDepreciationAmount += monthlyDepreciation;

            depreciationTransactions.Add(new DepreciationTransaction
            {
                FixedAssetId = asset.Id,
                Amount = monthlyDepreciation,
                DepreciationDate = periodStart,
                Notes = $"Depreciation for {periodStr} ({asset.DepreciationMethod})",
                CreatedAt = DateTime.UtcNow
            });

            // Update asset accumulated depreciation
            asset.AccumulatedDepreciation += monthlyDepreciation;
            asset.CalculateNetBookValue();
            _fixedAssetRepo.Update(asset);
        }

        if (totalDepreciationAmount <= 0)
        {
            throw new FixedAssetsDomainException("Calculated depreciation amount is zero for all assets.");
        }

        // 4. Create Journal Entry
        var journalEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryDate = DateTime.UtcNow,
            EntryNumber = $"DEP-{periodStr}-{Guid.NewGuid().ToString()[..4]}",
            Description = $"Monthly Depreciation for {periodStr}",
            Status = JournalEntryStatus.Posted,
            ReferenceType = "Depreciation",
            ReferenceNumber = periodStr,
            TotalDebit = totalDepreciationAmount,
            TotalCredit = totalDepreciationAmount,
            Lines = new List<JournalEntryLine>
            {
                new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    AccountId = expenseAcc.Id,
                    AccountCode = expenseAcc.Code,
                    AccountName = expenseAcc.Name,
                    DebitAmount = totalDepreciationAmount,
                    CreditAmount = 0,
                    Description = $"Depreciation Expense {periodStr}"
                },
                new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    AccountId = accumulatedAcc.Id,
                    AccountCode = accumulatedAcc.Code,
                    AccountName = accumulatedAcc.Name,
                    DebitAmount = 0,
                    CreditAmount = totalDepreciationAmount,
                    Description = $"Accumulated Depreciation {periodStr}"
                }
            }
        };

        // 5. Save Transaction
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _journalRepo.AddAsync(journalEntry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var tx in depreciationTransactions)
            {
                tx.JournalEntryId = journalEntry.Id;
                await _depreciationRepo.AddAsync(tx);
            }

            // Add Domain Event
            journalEntry.AddDomainEvent(new EnterpriseERP.Application.Features.FixedAssets.Events.DepreciationProcessedEvent(
                request.Year, request.Month, totalDepreciationAmount));

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        return journalEntry.Id;
    }
}
