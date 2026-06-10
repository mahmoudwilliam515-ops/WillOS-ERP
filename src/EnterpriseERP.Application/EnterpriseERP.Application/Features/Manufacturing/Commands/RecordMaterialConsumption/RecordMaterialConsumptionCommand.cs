using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.RecordMaterialConsumption;

public class RecordMaterialConsumptionCommand : IRequest<Result<bool>>
{
    public Guid ProductionOrderId { get; set; }
    public Guid MaterialId { get; set; }
    public decimal Quantity { get; set; }
}
