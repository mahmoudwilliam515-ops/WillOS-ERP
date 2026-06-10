using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Inventory.Commands.CreateInventoryAdjustment;

public class CreateInventoryAdjustmentCommandHandler
    : IRequestHandler<CreateInventoryAdjustmentCommand, Result<Guid>>
{
    private readonly IGenericRepository<InventoryAdjustment> _adjustmentRepo;
    private readonly IGenericRepository<Item> _itemRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInventoryAdjustmentCommandHandler(
        IGenericRepository<InventoryAdjustment> adjustmentRepo,
        IGenericRepository<Item> itemRepo,
        IUnitOfWork unitOfWork)
    {
        _adjustmentRepo = adjustmentRepo;
        _itemRepo       = itemRepo;
        _unitOfWork     = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateInventoryAdjustmentCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
            throw new InventoryDomainException("Adjustment quantity must be greater than zero.");

        var item = await _itemRepo.GetByIdAsync(request.ItemId)
            ?? throw new InventoryDomainException("Item not found.");

        var adjustmentType = (AdjustmentType)request.Type;

        var adjustment = new InventoryAdjustment
        {
            AdjustmentNumber = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}",
            WarehouseId      = request.WarehouseId,
            ItemId           = request.ItemId,
            Quantity         = request.Quantity,
            Type             = adjustmentType,
            Reason           = request.Reason,
            AdjustmentDate   = DateTime.UtcNow
        };

        await _adjustmentRepo.AddAsync(adjustment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(adjustment.Id);
    }
}
