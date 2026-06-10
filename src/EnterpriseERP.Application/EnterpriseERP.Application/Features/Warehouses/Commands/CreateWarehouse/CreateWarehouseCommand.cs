using EnterpriseERP.Application.Features.Warehouses.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Warehouses.Commands.CreateWarehouse;

public class CreateWarehouseCommand : IRequest<WarehouseDto>
{
    public string Name { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string? Address { get; set; }
    public bool IsMain { get; set; }
}
