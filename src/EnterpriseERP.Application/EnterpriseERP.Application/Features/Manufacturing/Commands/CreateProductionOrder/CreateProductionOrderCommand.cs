using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.CreateProductionOrder;

public record CreateProductionOrderCommand : IRequest<Result<Guid>>
{
    public Guid BillOfMaterialsId { get; init; }
    public Guid ProductId { get; init; }
    public Guid WarehouseId { get; init; }
    public decimal PlannedQuantity { get; init; }
    public DateTime? StartDate { get; init; }
    public bool ScheduleAutomatically { get; init; } = true;
}
