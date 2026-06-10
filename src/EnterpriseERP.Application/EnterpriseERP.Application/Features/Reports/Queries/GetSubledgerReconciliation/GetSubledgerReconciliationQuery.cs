using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Reports.Queries.GetSubledgerReconciliation;

public record GetSubledgerReconciliationQuery(DateTime AsOfDate) : IRequest<SubledgerReconciliationResult>;

public class SubledgerReconciliationResult
{
    public ReconciliationItem AR { get; set; } = new();
    public ReconciliationItem AP { get; set; } = new();
    public ReconciliationItem Inventory { get; set; } = new();
}

public class ReconciliationItem
{
    public decimal SubledgerTotal { get; set; }
    public decimal GLAccountTotal { get; set; }
    public decimal Difference => SubledgerTotal - GLAccountTotal;
    public string Status => Math.Abs(Difference) < 0.01m ? "Balanced" : "Discrepancy";
}

public class GetSubledgerReconciliationQueryHandler : IRequestHandler<GetSubledgerReconciliationQuery, SubledgerReconciliationResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSubledgerReconciliationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SubledgerReconciliationResult> Handle(GetSubledgerReconciliationQuery request, CancellationToken cancellationToken)
    {
        var result = new SubledgerReconciliationResult();

        // 1. AR Reconciliation
        var arSubledger = await _unitOfWork.Repository<Customer>().Query().SumAsync(c => c.Balance, cancellationToken);
        var arMapping = await _unitOfWork.Repository<AccountMapping>().Query()
            .FirstOrDefaultAsync(m => m.PostingKey == PostingKey.AR_RECEIVABLE, cancellationToken);
        
        decimal arGL = 0;
        if (arMapping != null)
        {
            arGL = await _unitOfWork.Repository<JournalEntryLine>().Query()
                .Where(l => l.AccountId == arMapping.AccountId && l.JournalEntry.Status == JournalEntryStatus.Posted && l.JournalEntry.EntryDate <= request.AsOfDate)
                .SumAsync(l => l.DebitAmount - l.CreditAmount, cancellationToken);
        }
        result.AR = new ReconciliationItem { SubledgerTotal = arSubledger, GLAccountTotal = arGL };

        // 2. AP Reconciliation
        var apSubledger = await _unitOfWork.Repository<Supplier>().Query().SumAsync(s => s.Balance, cancellationToken);
        var apMapping = await _unitOfWork.Repository<AccountMapping>().Query()
            .FirstOrDefaultAsync(m => m.PostingKey == PostingKey.AP_PAYABLE, cancellationToken);
        
        decimal apGL = 0;
        if (apMapping != null)
        {
            apGL = await _unitOfWork.Repository<JournalEntryLine>().Query()
                .Where(l => l.AccountId == apMapping.AccountId && l.JournalEntry.Status == JournalEntryStatus.Posted && l.JournalEntry.EntryDate <= request.AsOfDate)
                .SumAsync(l => l.CreditAmount - l.DebitAmount, cancellationToken); // AP is Credit positive
        }
        result.AP = new ReconciliationItem { SubledgerTotal = apSubledger, GLAccountTotal = apGL };

        // 3. Inventory Reconciliation
        var invSubledger = await _unitOfWork.Repository<InventoryTransaction>().Query()
            .Where(t => t.TransactionDate <= request.AsOfDate)
            .SumAsync(t => t.TotalCost, cancellationToken);

        var invMapping = await _unitOfWork.Repository<AccountMapping>().Query()
            .FirstOrDefaultAsync(m => m.PostingKey == PostingKey.INVENTORY_ASSET, cancellationToken);
        
        decimal invGL = 0;
        if (invMapping != null)
        {
            invGL = await _unitOfWork.Repository<JournalEntryLine>().Query()
                .Where(l => l.AccountId == invMapping.AccountId && l.JournalEntry.Status == JournalEntryStatus.Posted && l.JournalEntry.EntryDate <= request.AsOfDate)
                .SumAsync(l => l.DebitAmount - l.CreditAmount, cancellationToken);
        }
        result.Inventory = new ReconciliationItem { SubledgerTotal = invSubledger, GLAccountTotal = invGL };

        return result;
    }
}
