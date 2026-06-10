using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Maintenance.Commands.StartWorkOrder;

public class StartWorkOrderCommand : IRequest<Result<bool>>
{
    public Guid WorkOrderId { get; set; }
}
