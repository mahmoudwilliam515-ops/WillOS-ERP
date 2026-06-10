using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.StartProductionOrder;

public class StartProductionOrderCommand : IRequest<Result<bool>>
{
    public Guid ProductionOrderId { get; set; }
}
