using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Maintenance.Commands.CompleteWorkOrder;

public class CompleteWorkOrderCommand : IRequest<Result<bool>>
{
    public Guid WorkOrderId { get; set; }
    public string ResolutionNotes { get; set; } = string.Empty;
    public decimal TotalPartsCost { get; set; }
    public decimal TotalLaborCost { get; set; }
}
