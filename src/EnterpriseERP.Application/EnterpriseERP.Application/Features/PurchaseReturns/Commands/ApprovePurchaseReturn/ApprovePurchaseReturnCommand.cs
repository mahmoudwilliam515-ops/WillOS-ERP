using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Domain.Procurement;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.PurchaseReturns.Commands.ApprovePurchaseReturn;

public record ApprovePurchaseReturnCommand(Guid PurchaseReturnId) : IRequest<bool>;

public class ApprovePurchaseReturnCommandHandler : IRequestHandler<ApprovePurchaseReturnCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IInventoryPostingService _inventoryPostingService;

    public ApprovePurchaseReturnCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountingPostingService accountingPostingService,
        IInventoryPostingService inventoryPostingService)
    {
        _unitOfWork = unitOfWork;
        _accountingPostingService = accountingPostingService;
        _inventoryPostingService = inventoryPostingService;
    }

    public async Task<bool> Handle(ApprovePurchaseReturnCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var purchaseReturn = await _unitOfWork.Repository<PurchaseReturn>().Query()
                .Include(p => p.Lines)
                .FirstOrDefaultAsync(p => p.Id == request.PurchaseReturnId, cancellationToken);
            if (purchaseReturn == null)
                return false;
            
            if (purchaseReturn.Status == PurchaseReturnStatus.Approved)
                return true;

            purchaseReturn.Approve(Guid.Empty); // TODO: pass current user ID

            _unitOfWork.Repository<PurchaseReturn>().Update(purchaseReturn);

            var inventoryTxns = await _inventoryPostingService.PostPurchaseReturnAsync(purchaseReturn, cancellationToken);
            foreach (var txn in inventoryTxns)
            {
                await _unitOfWork.Repository<InventoryTransaction>().AddAsync(txn);
            }

            var journalEntry = await _accountingPostingService.PostPurchaseReturnAsync(purchaseReturn, cancellationToken);
            await _unitOfWork.Repository<JournalEntry>().AddAsync(journalEntry);
            foreach (var line in journalEntry.Lines)
                await _unitOfWork.Repository<JournalEntryLine>().AddAsync(line);

            var debitNote = new DebitNote
            {
                Id = Guid.NewGuid(),
                NoteNumber = $"DN-{purchaseReturn.ReturnNumber}",
                NoteDate = purchaseReturn.ReturnDate,
                SupplierId = purchaseReturn.SupplierId,
                PurchaseReturnId = purchaseReturn.Id,
                Amount = purchaseReturn.TotalAmount,
                Reason = purchaseReturn.Reason,
                Status = 0 // Draft
            };
            await _unitOfWork.Repository<DebitNote>().AddAsync(debitNote);

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
