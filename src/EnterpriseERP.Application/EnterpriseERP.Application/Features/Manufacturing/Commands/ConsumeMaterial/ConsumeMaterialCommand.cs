using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.ConsumeMaterial;

public record ConsumeMaterialCommand(Guid ProductionOrderId, Guid MaterialId, decimal Quantity) : IRequest<bool>;

public class ConsumeMaterialCommandHandler : IRequestHandler<ConsumeMaterialCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConsumeMaterialCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ConsumeMaterialCommand request, CancellationToken cancellationToken)
    {
        var orderMaterial = await _unitOfWork.Repository<ProductionOrderMaterial>().Query()
            .FirstOrDefaultAsync(m => m.ProductionOrderId == request.ProductionOrderId && m.RawMaterialId == request.MaterialId, cancellationToken);

        if (orderMaterial == null) return false;

        // 1. Update Actual Quantity
        orderMaterial.ActualQuantity += request.Quantity;
        _unitOfWork.Repository<ProductionOrderMaterial>().Update(orderMaterial);

        // 2. Create Inventory Transaction (Deduction)
        var inventoryTx = new InventoryTransaction
        {
            Id = Guid.NewGuid(),
            ItemId = request.MaterialId,
            Quantity = -request.Quantity, // Outgoing
            Type = TransactionType.ManufacturingOut,
            ReferenceNumber = request.ProductionOrderId.ToString(),
            TransactionDate = DateTime.UtcNow,
            WarehouseId = Guid.Empty, // Should be passed in real scenario
            UnitCost = orderMaterial.UnitCost,
            TotalCost = request.Quantity * orderMaterial.UnitCost
        };

        await _unitOfWork.Repository<InventoryTransaction>().AddAsync(inventoryTx);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
