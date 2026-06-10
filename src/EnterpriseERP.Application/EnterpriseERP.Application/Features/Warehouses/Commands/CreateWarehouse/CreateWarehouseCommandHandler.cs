using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Warehouses.DTOs;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;

namespace EnterpriseERP.Application.Features.Warehouses.Commands.CreateWarehouse;

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateWarehouseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<WarehouseDto> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            BranchId = request.BranchId,
            Address = request.Address ?? string.Empty,
            IsMain = request.IsMain,
            IsActive = true
        };

        await _unitOfWork.Repository<Warehouse>().AddAsync(warehouse);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WarehouseDto
        {
            Id = warehouse.Id,
            Name = warehouse.Name,
            BranchId = warehouse.BranchId,
            Address = warehouse.Address,
            IsMain = warehouse.IsMain,
            IsActive = warehouse.IsActive
        };
    }
}
