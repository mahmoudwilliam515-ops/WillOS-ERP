using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Manufacturing.DTOs;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Manufacturing.Queries.GetProductionProfitabilityReport;

public record GetProductionProfitabilityReportQuery : IRequest<List<ProductionProfitabilityDto>>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class GetProductionProfitabilityReportQueryHandler : IRequestHandler<GetProductionProfitabilityReportQuery, List<ProductionProfitabilityDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductionProfitabilityReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductionProfitabilityDto>> Handle(GetProductionProfitabilityReportQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<ProductionOrder>().Query()
            .Include(o => o.Product)
            .Where(o => o.Status == ProductionOrderStatus.Completed);

        if (request.StartDate.HasValue)
            query = query.Where(o => o.ActualEndDate >= request.StartDate.Value);
        
        if (request.EndDate.HasValue)
            query = query.Where(o => o.ActualEndDate <= request.EndDate.Value);

        var orders = await query.ToListAsync(cancellationToken);

        var report = orders.Select(o => new ProductionProfitabilityDto
        {
            ProductionOrderId = o.Id,
            OrderNumber = o.OrderNumber,
            ProductName = o.Product.NameEn,
            Quantity = o.ProducedQuantity,
            CompletionDate = o.ActualEndDate ?? DateTime.UtcNow,
            ActualMaterialCost = o.TotalMaterialCost,
            ActualLaborCost = o.TotalLaborCost,
            PlannedTotalCost = o.PlannedMaterialCost + o.PlannedLaborCost,
            EstimatedRevenue = 0 // SellPrice not available in Item entity yet
        }).ToList();

        return report;
    }
}
