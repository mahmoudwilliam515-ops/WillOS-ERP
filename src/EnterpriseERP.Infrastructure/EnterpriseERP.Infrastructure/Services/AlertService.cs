using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Infrastructure.Services;

public class AlertService : IAlertService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public AlertService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task CheckManufacturingAlertsAsync(CancellationToken cancellationToken = default)
    {
        await CheckMaterialShortagesAsync(cancellationToken);
        await CheckProductionDelaysAsync(cancellationToken);
        await CheckWorkCenterOverloadsAsync(cancellationToken);
    }

    private async Task CheckMaterialShortagesAsync(CancellationToken cancellationToken)
    {
        // 1. Find all active production order materials
        var materialsNeeded = await _unitOfWork.Repository<ProductionOrderMaterial>().Query()
            .Include(m => m.ProductionOrder)
            .Include(m => m.RawMaterial)
            .Where(m => m.ProductionOrder.Status == ProductionOrderStatus.Planned || m.ProductionOrder.Status == ProductionOrderStatus.InProgress)
            .ToListAsync(cancellationToken);

        foreach (var material in materialsNeeded)
        {
            // Simple check: current stock vs planned
            var currentStock = await _unitOfWork.Repository<InventoryTransaction>().Query()
                .Where(t => t.ItemId == material.RawMaterialId)
                .SumAsync(t => t.Quantity, cancellationToken);

            if (currentStock < material.PlannedQuantity)
            {
                await _notificationService.SendSystemAlertAsync(
                    "نقص في المواد الخام",
                    $"أمر الإنتاج {material.ProductionOrder.OrderNumber} يحتاج {material.PlannedQuantity} من {material.RawMaterial.Name} ولكن المتاح {currentStock} فقط.",
                    "warning",
                    $"/manufacturing/orders/{material.ProductionOrderId}/tracking"
                );
            }
        }
    }

    private async Task CheckProductionDelaysAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var delayedStages = await _unitOfWork.Repository<ProductionOrderStage>().Query()
            .Include(s => s.ProductionOrder)
            .Where(s => s.Status != ProductionStageStatus.Completed && s.EndDate < now)
            .ToListAsync(cancellationToken);

        foreach (var stage in delayedStages)
        {
            await _notificationService.SendSystemAlertAsync(
                "تأخير في الإنتاج",
                $"المرحلة {stage.Name} في أمر الإنتاج {stage.ProductionOrder.OrderNumber} تجاوزت الموعد المخطط لها.",
                "error",
                $"/manufacturing/orders/{stage.ProductionOrderId}/tracking"
            );
        }
    }

    private async Task CheckWorkCenterOverloadsAsync(CancellationToken cancellationToken)
    {
        var workCenters = await _unitOfWork.Repository<WorkCenter>().Query().ToListAsync(cancellationToken);
        var next7Days = DateTime.UtcNow.AddDays(7);

        foreach (var wc in workCenters)
        {
            var loads = await _unitOfWork.Repository<ProductionOrderStage>().Query()
                .Where(s => s.WorkCenterId == wc.Id && s.StartDate >= DateTime.UtcNow && s.StartDate <= next7Days)
                .GroupBy(s => s.StartDate.Value.Date)
                .Select(g => new { Date = g.Key, TotalHours = g.Sum(s => s.EstimatedHours) })
                .ToListAsync(cancellationToken);

            foreach (var load in loads)
            {
                if (load.TotalHours > 8) // Assuming 8h capacity
                {
                    await _notificationService.SendSystemAlertAsync(
                        "تحميل زائد على مركز العمل",
                        $"مركز العمل {wc.Name} لديه تحميل زائد ({load.TotalHours} ساعة) بتاريخ {load.Date:yyyy-MM-dd}.",
                        "warning",
                        "/manufacturing/scheduler"
                    );
                }
            }
        }
    }
}
