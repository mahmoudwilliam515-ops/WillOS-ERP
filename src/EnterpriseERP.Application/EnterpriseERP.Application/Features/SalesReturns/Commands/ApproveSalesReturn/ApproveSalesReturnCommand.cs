using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.SalesReturns.Commands.ApproveSalesReturn;

public record ApproveSalesReturnCommand(Guid SalesReturnId) : IRequest<Result<bool>>;

public class ApproveSalesReturnCommandHandler : IRequestHandler<ApproveSalesReturnCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IInventoryPostingService _inventoryPostingService;

    public ApproveSalesReturnCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountingPostingService accountingPostingService,
        IInventoryPostingService inventoryPostingService)
    {
        _unitOfWork = unitOfWork;
        _accountingPostingService = accountingPostingService;
        _inventoryPostingService = inventoryPostingService;
    }

    public async Task<Result<bool>> Handle(ApproveSalesReturnCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var salesReturn = await _unitOfWork.Repository<SalesReturn>().Query()
                .Include(r => r.Lines)
                .FirstOrDefaultAsync(r => r.Id == request.SalesReturnId, cancellationToken);

            if (salesReturn == null)
                return Result.Failure<bool>(new Error("SalesReturn.NotFound", "Sales Return not found."));
            
            if (salesReturn.Status == SalesReturnStatus.Approved)
                return Result.Success(true);

            // Load original invoice lines to get the true COGS (ItemCost)
            var invoiceLineIds = salesReturn.Lines.Select(l => l.SalesInvoiceLineId).Distinct().ToList();
            var invoiceLines = await _unitOfWork.Repository<SalesInvoiceLine>().FindAsync(l => invoiceLineIds.Contains(l.Id));
            
            foreach (var line in salesReturn.Lines)
            {
                var originalLine = invoiceLines.FirstOrDefault(il => il.Id == line.SalesInvoiceLineId);
                if (originalLine != null)
                {
                    line.SetOriginalItemCost(originalLine.ItemCost);
                }
            }

            salesReturn.Approve();
            _unitOfWork.Repository<SalesReturn>().Update(salesReturn);

            // 1. Inventory Posting (Restock)
            var inventoryTxns = await _inventoryPostingService.PostSalesReturnAsync(salesReturn, cancellationToken);
            foreach (var txn in inventoryTxns)
            {
                await _unitOfWork.Repository<InventoryTransaction>().AddAsync(txn);
            }

            // 2. Accounting Posting (Credit Note / AR reduction / COGS reversal)
            var journalEntry = await _accountingPostingService.PostSalesReturnAsync(salesReturn, cancellationToken);
            await _unitOfWork.Repository<JournalEntry>().AddAsync(journalEntry);

            var creditNote = new CreditNote
            {
                Id = Guid.NewGuid(),
                NoteNumber = $"CN-{salesReturn.ReturnNumber}",
                NoteDate = salesReturn.ReturnDate,
                CustomerId = salesReturn.CustomerId,
                SalesReturnId = salesReturn.Id,
                Amount = salesReturn.TotalAmount,
                Reason = salesReturn.Reason,
                Status = 0 // Draft
            };
            await _unitOfWork.Repository<CreditNote>().AddAsync(creditNote);

            await _unitOfWork.CommitTransactionAsync();
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Failure<bool>(new Error("SalesReturn.ApprovalError", ex.Message));
        }
    }
}
