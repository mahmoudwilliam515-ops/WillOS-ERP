using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Manufacturing.DTOs;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Manufacturing.Queries.GetProductionOrders;

public class GetProductionOrdersQueryHandler : IRequestHandler<GetProductionOrdersQuery, Result<IEnumerable<ProductionOrderDto>>>
{
    private readonly IGenericRepository<ProductionOrder> _repository;

    public GetProductionOrdersQueryHandler(IGenericRepository<ProductionOrder> repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<ProductionOrderDto>>> Handle(GetProductionOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query()
            .Include(o => o.Product)
            .AsQueryable();

        if (request.ProductId.HasValue)
            query = query.Where(o => o.ProductId == request.ProductId.Value);

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<ProductionOrderStatus>(request.Status, true, out var status))
                query = query.Where(o => o.Status == status);
        }

        if (request.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= request.ToDate.Value);

        var orders = await query.ToListAsync(cancellationToken);
        
        var dtos = orders.Select(o => new ProductionOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            ProductId = o.ProductId,
            ProductName = o.Product?.Name,
            BillOfMaterialsId = o.BillOfMaterialsId,
            PlannedQuantity = o.Quantity,
            ProducedQuantity = o.ProducedQuantity,
            Status = o.Status.ToString(),
            StartDate = o.ActualStartDate,
            EndDate = o.ActualEndDate,
            PlannedMaterialCost = o.PlannedMaterialCost,
            PlannedLaborCost = o.PlannedLaborCost,
            PlannedTotalCost = o.PlannedTotalCost,
            TotalMaterialCost = o.TotalMaterialCost,
            TotalLaborCost = o.TotalLaborCost,
            TotalCost = o.TotalCost,
            CreatedAt = o.CreatedAt
        });

        return Result<IEnumerable<ProductionOrderDto>>.Success(dtos);
    }
}
