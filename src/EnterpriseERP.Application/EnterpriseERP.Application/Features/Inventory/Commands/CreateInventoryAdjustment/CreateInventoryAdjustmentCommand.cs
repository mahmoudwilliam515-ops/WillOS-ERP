using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Inventory.Commands.CreateInventoryAdjustment;

public record CreateInventoryAdjustmentCommand : IRequest<Result<Guid>>
{
    public Guid WarehouseId { get; init; }
    public Guid ItemId      { get; init; }
    public decimal Quantity { get; init; }
    public int Type         { get; init; }   // Maps to AdjustmentType enum
    public string Reason    { get; init; } = string.Empty;
}
