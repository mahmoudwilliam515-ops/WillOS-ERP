using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Events.Manufacturing;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Manufacturing.Events;

public class MaterialConsumedEventHandler : INotificationHandler<MaterialConsumedEvent>
{
    private readonly IGenericRepository<InventoryTransaction> _transactionRepo;

    public MaterialConsumedEventHandler(IGenericRepository<InventoryTransaction> transactionRepo)
    {
        _transactionRepo = transactionRepo;
    }

    public async Task Handle(MaterialConsumedEvent notification, CancellationToken cancellationToken)
    {
        // Record material consumption as an inventory reduction
        var transaction = new InventoryTransaction
        {
            ItemId = notification.MaterialId,
            WarehouseId = notification.WarehouseId,
            ReferenceId = notification.ProductionOrderId,
            ReferenceType = "ProductionOrder",
            ReferenceNumber = notification.ProductionOrderId.ToString(),
            Type = TransactionType.ManufacturingOut, // Reduction
            Quantity = -notification.Quantity,       // Negative for OUT
            UnitCost = notification.UnitCost,
            TotalCost = notification.Quantity * notification.UnitCost,
            TransactionDate = DateTime.UtcNow,
            Notes = $"Material consumed for Production Order {notification.ProductionOrderId}"
        };

        await _transactionRepo.AddAsync(transaction);
        
        // Note: UnitOfWork.SaveChangesAsync() is handled by the calling command
    }
}
