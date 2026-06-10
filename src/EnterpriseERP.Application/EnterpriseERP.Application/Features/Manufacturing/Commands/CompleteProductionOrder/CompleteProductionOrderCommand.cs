using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.CompleteProductionOrder;

public class CompleteProductionOrderCommand : IRequest<Result<bool>>
{
    public Guid ProductionOrderId { get; set; }
    public decimal ProducedQuantity { get; set; }
    public decimal TotalLaborCost { get; set; }
    public decimal TotalMaterialCost { get; set; }
    public Guid WarehouseId { get; set; }
}
