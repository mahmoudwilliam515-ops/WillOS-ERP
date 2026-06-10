using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Warehouses.DTOs;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;

namespace EnterpriseERP.Application.Features.Warehouses.Queries.GetAllWarehouses;

public class GetAllWarehousesQueryHandler : IRequestHandler<GetAllWarehousesQuery, IEnumerable<WarehouseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllWarehousesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<WarehouseDto>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
    {
        var warehouses = await _unitOfWork.Repository<Warehouse>().GetAllAsync();
        
        return warehouses.Select(w => new WarehouseDto
        {
            Id = w.Id,
            Name = w.Name,
            BranchId = w.BranchId,
            Address = w.Address,
            IsMain = w.IsMain,
            IsActive = w.IsActive
        });
    }
}
