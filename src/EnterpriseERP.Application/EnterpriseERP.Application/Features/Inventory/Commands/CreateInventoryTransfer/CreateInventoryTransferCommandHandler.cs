using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Inventory.Commands.CreateInventoryTransfer;

public class CreateInventoryTransferCommandHandler
    : IRequestHandler<CreateInventoryTransferCommand, Result<Guid>>
{
    private readonly IGenericRepository<InventoryTransfer> _transferRepo;
    private readonly IGenericRepository<Warehouse> _warehouseRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInventoryTransferCommandHandler(
        IGenericRepository<InventoryTransfer> transferRepo,
        IGenericRepository<Warehouse> warehouseRepo,
        IUnitOfWork unitOfWork)
    {
        _transferRepo  = transferRepo;
        _warehouseRepo = warehouseRepo;
        _unitOfWork    = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateInventoryTransferCommand request,
        CancellationToken cancellationToken)
    {
        if (request.FromWarehouseId == request.ToWarehouseId)
            throw new InventoryDomainException("Source and destination warehouses must be different.");

        var fromWarehouse = await _warehouseRepo.GetByIdAsync(request.FromWarehouseId)
            ?? throw new InventoryDomainException("Source warehouse not found.");

        var toWarehouse = await _warehouseRepo.GetByIdAsync(request.ToWarehouseId)
            ?? throw new InventoryDomainException("Destination warehouse not found.");

        var transfer = new InventoryTransfer
        {
            TransferNumber  = $"TRF-{DateTime.UtcNow:yyyyMMddHHmmss}",
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId   = request.ToWarehouseId,
            TransferDate    = request.TransferDate == default ? DateTime.UtcNow : request.TransferDate,
            Status          = InventoryTransferStatus.Draft,
            Remarks         = request.Remarks
        };

        await _transferRepo.AddAsync(transfer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(transfer.Id);
    }
}
