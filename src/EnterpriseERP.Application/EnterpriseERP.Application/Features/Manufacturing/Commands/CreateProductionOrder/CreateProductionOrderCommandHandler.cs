using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.CreateProductionOrder;

public class CreateProductionOrderCommandHandler : IRequestHandler<CreateProductionOrderCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductionSchedulerService _schedulerService;

    public CreateProductionOrderCommandHandler(IUnitOfWork unitOfWork, IProductionSchedulerService schedulerService)
    {
        _unitOfWork = unitOfWork;
        _schedulerService = schedulerService;
    }

    public async Task<Result<Guid>> Handle(CreateProductionOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Get BOM with Lines (and their RawMaterials) and Stages
        var bomQuery = _unitOfWork.Repository<BillOfMaterials>().Query()
            .Include(b => b.Lines)
                .ThenInclude(l => l.RawMaterial)
            .Include(b => b.Stages);

        var bom = bomQuery.Provider is IAsyncQueryProvider
            ? await bomQuery.FirstOrDefaultAsync(b => b.Id == request.BillOfMaterialsId, cancellationToken)
            : bomQuery.FirstOrDefault(b => b.Id == request.BillOfMaterialsId);

        if (bom == null)
            return Result.Failure<Guid>(new Error("BOM.NotFound", "Bill of Materials not found."));

        // Calculate Planned Costs
        decimal plannedMaterialCost = bom.Lines.Sum(l => l.Quantity * request.PlannedQuantity * l.RawMaterial.CostPerUnit);
        decimal plannedLaborCost = bom.Stages.Sum(s => s.EstimatedHours * request.PlannedQuantity * s.CostPerHour);

        // 2. Create Production Order
        var order = new ProductionOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmm}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
            BillOfMaterialsId = request.BillOfMaterialsId,
            ProductId = request.ProductId,
            Quantity = request.PlannedQuantity,
            Status = ProductionOrderStatus.Draft,
            WarehouseId = request.WarehouseId,
            PlannedStartDate = request.StartDate ?? DateTime.UtcNow,
            PlannedMaterialCost = plannedMaterialCost,
            PlannedLaborCost = plannedLaborCost,
            
            // 3. Populate Materials from BOM
            Materials = bom.Lines.Select(l => new ProductionOrderMaterial
            {
                Id = Guid.NewGuid(),
                RawMaterialId = l.RawMaterialId,
                PlannedQuantity = l.Quantity * request.PlannedQuantity,
                ActualQuantity = 0,
                UnitCost = l.RawMaterial.CostPerUnit
            }).ToList(),

            // 4. Populate Stages from BOM
            Stages = bom.Stages.Select(s => new ProductionOrderStage
            {
                Id = Guid.NewGuid(),
                Name = s.Name,
                Sequence = s.Sequence,
                EstimatedHours = s.EstimatedHours * request.PlannedQuantity,
                CostPerHour = s.CostPerHour,
                WorkCenterId = s.WorkCenterId, // Use the WorkCenterId from the BOM stage
                Status = ProductionStageStatus.Pending
            }).ToList()
        };

        // 5. Apply Automatic Scheduling if requested
        if (request.ScheduleAutomatically)
        {
            await _schedulerService.ScheduleProductionOrderAsync(order, cancellationToken);
        }

        await _unitOfWork.Repository<ProductionOrder>().AddAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Id);
    }
}
