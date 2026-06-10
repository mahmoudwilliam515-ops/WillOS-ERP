using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Inventory.Commands.ReceiveTransfer;

public record ReceiveTransferCommand(Guid TransferOrderId) : IRequest<bool>;

public class ReceiveTransferCommandHandler : IRequestHandler<ReceiveTransferCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ReceiveTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ReceiveTransferCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var transfer = await _unitOfWork.Repository<TransferOrder>().GetByIdAsync(request.TransferOrderId);
            if (transfer == null || transfer.Status != TransferStatus.Shipped)
                return false;

            foreach (var line in transfer.Lines)
            {
                // 1. Transaction: Out from Transit
                var transitOutTx = new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    WarehouseId = transfer.FromWarehouseId,
                    ReferenceId = transfer.Id,
                    ReferenceType = "TransferOrder",
                    ReferenceNumber = transfer.TransferNumber,
                    Type = TransactionType.TransferTransit,
                    Quantity = -line.Quantity, // Out from transit
                    TransactionDate = DateTime.UtcNow,
                    Notes = $"Received transfer {transfer.TransferNumber} - clearing transit"
                };
                await _unitOfWork.Repository<InventoryTransaction>().AddAsync(transitOutTx);

                // 2. Transaction: Into Destination Warehouse
                var inTx = new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    WarehouseId = transfer.ToWarehouseId,
                    ReferenceId = transfer.Id,
                    ReferenceType = "TransferOrder",
                    ReferenceNumber = transfer.TransferNumber,
                    Type = TransactionType.TransferIn,
                    Quantity = line.Quantity, // Into destination
                    TransactionDate = DateTime.UtcNow,
                    Notes = $"Received transfer {transfer.TransferNumber}"
                };
                await _unitOfWork.Repository<InventoryTransaction>().AddAsync(inTx);
            }

            transfer.Status = TransferStatus.Received;
            _unitOfWork.Repository<TransferOrder>().Update(transfer);

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
