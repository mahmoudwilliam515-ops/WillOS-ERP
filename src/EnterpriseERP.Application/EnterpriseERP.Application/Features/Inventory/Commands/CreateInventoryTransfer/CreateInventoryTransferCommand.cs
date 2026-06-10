using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Inventory.Commands.CreateInventoryTransfer;

public record CreateInventoryTransferCommand : IRequest<Result<Guid>>
{
    public Guid FromWarehouseId { get; init; }
    public Guid ToWarehouseId   { get; init; }
    public DateTime TransferDate { get; init; }
    public string Remarks { get; init; } = string.Empty;
}
