using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Inventory.Commands.ShipTransfer;

public record ShipTransferCommand(Guid TransferOrderId) : IRequest<bool>;

public class ShipTransferCommandHandler : IRequestHandler<ShipTransferCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ShipTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ShipTransferCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var transfer = await _unitOfWork.Repository<TransferOrder>().GetByIdAsync(request.TransferOrderId);
            if (transfer == null || transfer.Status != TransferStatus.Draft)
                return false;

            foreach (var line in transfer.Lines)
            {
                // 1. Transaction: Out from Source Warehouse
                var outTx = new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    WarehouseId = transfer.FromWarehouseId,
                    ReferenceId = transfer.Id,
                    ReferenceType = "TransferOrder",
                    ReferenceNumber = transfer.TransferNumber,
                    Type = TransactionType.TransferOut,
                    Quantity = -line.Quantity, // Out
                    TransactionDate = DateTime.UtcNow,
                    Notes = $"Shipped transfer {transfer.TransferNumber}"
                };
                await _unitOfWork.Repository<InventoryTransaction>().AddAsync(outTx);

                // 2. Transaction: Into Transit (Virtual)
                var transitTx = new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    ItemId = line.ItemId,
                    WarehouseId = transfer.FromWarehouseId, // Still linked to source or a transit WH
                    ReferenceId = transfer.Id,
                    ReferenceType = "TransferOrder",
                    ReferenceNumber = transfer.TransferNumber,
                    Type = TransactionType.TransferTransit,
                    Quantity = line.Quantity, // Into Transit
                    TransactionDate = DateTime.UtcNow,
                    Notes = $"In-transit for {transfer.TransferNumber}"
                };
                await _unitOfWork.Repository<InventoryTransaction>().AddAsync(transitTx);
            }

            transfer.Status = TransferStatus.Shipped;
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
