using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Domain.Entities.Quality;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Manufacturing.Queries.GetManufacturingKpis;

public record GetManufacturingKpisQuery : IRequest<Result<ManufacturingKpisDto>>;

public record ManufacturingKpisDto
{
    public decimal ProductionEfficiency { get; init; }
    public decimal CostVariancePercentage { get; init; }
    public int ActiveWorkOrdersCount { get; init; }
    public decimal QualityPassRate { get; init; }
    public List<WorkCenterLoadDto> WorkCenterLoads { get; init; } = new();
}

public record WorkCenterLoadDto
{
    public string WorkCenterName { get; init; } = string.Empty;
    public int ActiveOrders { get; init; }
    public string Status { get; init; } = string.Empty;
}

public class GetManufacturingKpisQueryHandler : IRequestHandler<GetManufacturingKpisQuery, Result<ManufacturingKpisDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetManufacturingKpisQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ManufacturingKpisDto>> Handle(GetManufacturingKpisQuery request, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Repository<ProductionOrder>().Query().ToListAsync(cancellationToken);
        var inspections = await _unitOfWork.Repository<QualityInspection>().Query().ToListAsync(cancellationToken);
        var workCenters = await _unitOfWork.Repository<WorkCenter>().Query().ToListAsync(cancellationToken);

        // 1. Efficiency: Produced / Planned for completed orders
        var completedOrders = orders.Where(o => o.Status == ProductionOrderStatus.Completed).ToList();
        decimal efficiency = completedOrders.Any() 
            ? completedOrders.Average(o => o.Quantity > 0 ? (o.ProducedQuantity / o.Quantity) * 100 : 100)
            : 100;

        // 2. Cost Variance: (Actual - Planned) / Planned
        decimal totalPlanned = completedOrders.Sum(o => o.PlannedTotalCost);
        decimal totalActual = completedOrders.Sum(o => o.TotalCost);
        decimal costVariance = totalPlanned > 0 ? ((totalActual - totalPlanned) / totalPlanned) * 100 : 0;

        // 3. Quality Pass Rate
        var completedInspections = inspections.Where(i => i.Status != InspectionStatus.Pending).ToList();
        decimal passRate = completedInspections.Any()
            ? (decimal)completedInspections.Count(i => i.Status == InspectionStatus.Passed) / completedInspections.Count * 100
            : 100;

        // 4. Work Center Loads
        var wcLoads = workCenters.Select(wc => new WorkCenterLoadDto
        {
            WorkCenterName = wc.Name,
            Status = wc.Status.ToString(),
            ActiveOrders = orders.Count(o => o.Status == ProductionOrderStatus.InProgress && o.Stages.Any(s => s.WorkCenterId == wc.Id && s.Status == ProductionStageStatus.InProgress))
        }).ToList();

        return Result.Success(new ManufacturingKpisDto
        {
            ProductionEfficiency = efficiency,
            CostVariancePercentage = costVariance,
            ActiveWorkOrdersCount = orders.Count(o => o.Status == ProductionOrderStatus.InProgress),
            QualityPassRate = passRate,
            WorkCenterLoads = wcLoads
        });
    }
}
