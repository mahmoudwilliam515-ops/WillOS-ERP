using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Quality;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.ApproveGoodsReceiptNote;

public record ApproveGoodsReceiptNoteCommand(Guid GoodsReceiptNoteId) : IRequest<bool>;

public class ApproveGoodsReceiptNoteCommandHandler : IRequestHandler<ApproveGoodsReceiptNoteCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInventoryPostingService _inventoryPostingService;
    private readonly IAccountingPostingService _accountingPostingService;

    public ApproveGoodsReceiptNoteCommandHandler(
        IUnitOfWork unitOfWork, 
        IInventoryPostingService inventoryPostingService,
        IAccountingPostingService accountingPostingService)
    {
        _unitOfWork = unitOfWork;
        _inventoryPostingService = inventoryPostingService;
        _accountingPostingService = accountingPostingService;
    }

    public async Task<bool> Handle(ApproveGoodsReceiptNoteCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var grn = await _unitOfWork.Repository<GoodsReceiptNote>().GetByIdAsync(request.GoodsReceiptNoteId);
            if (grn == null || grn.Status == GoodsReceiptStatus.Received)
                return false;

            var lines = await _unitOfWork.Repository<GoodsReceiptLine>().FindAsync(l => l.GoodsReceiptNoteId == grn.Id);
            grn.Lines = lines.ToList();
            if (!grn.Lines.Any())
                return false;

            // 0. Quality Check Gate
            var pendingInspection = (await _unitOfWork.Repository<QualityInspection>()
                .FindAsync(i => i.ReferenceId == grn.Id && i.Type == InspectionType.PurchaseReceipt))
                .FirstOrDefault();

            if (pendingInspection != null && pendingInspection.Status != InspectionStatus.Passed)
            {
                throw new SalesDomainException($"Cannot approve GRN. Quality Inspection {pendingInspection.InspectionNumber} is {pendingInspection.Status}.");
            }

            grn.Status = GoodsReceiptStatus.Received;
            _unitOfWork.Repository<GoodsReceiptNote>().Update(grn);

            // 1. Inventory Posting
            var inventoryTxns = await _inventoryPostingService.PostGoodsReceiptNoteAsync(grn, cancellationToken);
            foreach (var txn in inventoryTxns)
            {
                await _unitOfWork.Repository<InventoryTransaction>().AddAsync(txn);
            }

            // 2. Accounting Posting (GRNI Accrual)
            await _accountingPostingService.PostGoodsReceiptNoteAsync(grn, cancellationToken);

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
