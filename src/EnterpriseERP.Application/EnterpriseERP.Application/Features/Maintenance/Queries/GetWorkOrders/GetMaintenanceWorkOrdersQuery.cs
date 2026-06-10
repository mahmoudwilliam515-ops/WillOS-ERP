using EnterpriseERP.Application.Features.Maintenance.DTOs;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System.Collections.Generic;

namespace EnterpriseERP.Application.Features.Maintenance.Queries.GetWorkOrders;

public record GetMaintenanceWorkOrdersQuery : IRequest<Result<IEnumerable<MaintenanceWorkOrderDto>>>;
