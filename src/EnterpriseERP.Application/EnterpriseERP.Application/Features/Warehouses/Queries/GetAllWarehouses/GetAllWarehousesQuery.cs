using EnterpriseERP.Application.Features.Warehouses.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Warehouses.Queries.GetAllWarehouses;

public class GetAllWarehousesQuery : IRequest<IEnumerable<WarehouseDto>>
{
}
