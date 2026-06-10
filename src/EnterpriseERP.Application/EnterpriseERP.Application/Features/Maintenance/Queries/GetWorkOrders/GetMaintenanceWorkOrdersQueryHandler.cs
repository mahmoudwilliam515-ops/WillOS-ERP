using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Maintenance.DTOs;
using EnterpriseERP.Domain.Entities.Maintenance;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Maintenance.Queries.GetWorkOrders;

public class GetMaintenanceWorkOrdersQueryHandler : IRequestHandler<GetMaintenanceWorkOrdersQuery, Result<IEnumerable<MaintenanceWorkOrderDto>>>
{
    private readonly IGenericRepository<MaintenanceWorkOrder> _repository;

    public GetMaintenanceWorkOrdersQueryHandler(IGenericRepository<MaintenanceWorkOrder> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<MaintenanceWorkOrderDto>>> Handle(GetMaintenanceWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repository.GetAllAsync();
        
        var dtos = orders.Select(o => new MaintenanceWorkOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            AssetId = o.AssetId,
            AssignedTechnicianId = o.AssignedTechnicianId,
            Description = o.Description,
            ResolutionNotes = o.ResolutionNotes,
            Status = o.Status.ToString(),
            StartDate = o.StartDate,
            CompletionDate = o.CompletionDate,
            TotalPartsCost = o.TotalPartsCost,
            TotalLaborCost = o.TotalLaborCost,
            TotalCost = o.TotalCost,
            CreatedAt = o.CreatedAt
        });

        return Result<IEnumerable<MaintenanceWorkOrderDto>>.Success(dtos);
    }
}
