using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Events.Manufacturing;
using EnterpriseERP.Application.Common.Interfaces.Services;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;


namespace EnterpriseERP.Application.Features.Inventory.EventHandlers;

public class ProductionOrderCompletedEventHandler : INotificationHandler<ProductionOrderCompletedEvent>
{
    private readonly IGenericRepository<InventoryTransaction> _transactionRepo;
    private readonly IGenericRepository<Item> _itemRepo;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IGenericRepository<EnterpriseERP.Domain.Entities.Accounting.JournalEntry> _journalRepo;

    public ProductionOrderCompletedEventHandler(
        IGenericRepository<InventoryTransaction> transactionRepo,
        IGenericRepository<Item> itemRepo,
        IAccountingPostingService accountingPostingService,
        IGenericRepository<EnterpriseERP.Domain.Entities.Accounting.JournalEntry> journalRepo)
    {
        _transactionRepo = transactionRepo;
        _itemRepo = itemRepo;
        _accountingPostingService = accountingPostingService;
        _journalRepo = journalRepo;
    }

    public async Task Handle(ProductionOrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        var unitCost = notification.TotalCost / notification.ProducedQuantity;

        var transaction = new InventoryTransaction
        {
            ItemId = notification.ProductId,
            WarehouseId = notification.WarehouseId,
            ReferenceId = notification.ProductionOrderId,
            ReferenceType = "ProductionOrder",
            ReferenceNumber = notification.ProductionOrderId.ToString(),
            Type = TransactionType.ManufacturingIn,
            Quantity = notification.ProducedQuantity,
            UnitCost = unitCost,
            TotalCost = notification.TotalCost,
            TransactionDate = DateTime.UtcNow,
            Notes = "Generated from completed production order"
        };

        await _transactionRepo.AddAsync(transaction);
        
        // Post Accounting Journal Entry for WIP -> Finished Goods
        var journalEntry = await _accountingPostingService.PostProductionOrderAsync(
            notification.ProductionOrderId, 
            notification.TotalCost, 
            notification.TotalMaterialCost, 
            notification.TotalLaborCost, 
            notification.ProductionOrderId.ToString(), 
            cancellationToken);
            
        await _journalRepo.AddAsync(journalEntry);

        // Note: _unitOfWork.SaveChangesAsync() is NOT called here because this is executed 
        // within the scope of the original UnitOfWork.SaveChangesAsync() loop in MediatR pipeline.
    }
}
