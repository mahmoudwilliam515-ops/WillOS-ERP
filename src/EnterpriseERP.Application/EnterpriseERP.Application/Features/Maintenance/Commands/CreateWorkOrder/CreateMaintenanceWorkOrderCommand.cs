using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Maintenance.Commands.CreateWorkOrder;

public record CreateMaintenanceWorkOrderCommand : IRequest<Result<Guid>>
{
    public Guid AssetId { get; init; }
    public string Description { get; init; } = string.Empty;
    public Guid? AssignedTechnicianId { get; init; }
}
