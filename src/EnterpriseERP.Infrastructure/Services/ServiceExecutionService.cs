using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Beauty;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Infrastructure.Services;

/// <summary>
/// Service Execution Engine Implementation.
/// FEFO (First Expiry First Out) for beauty product consumption.
/// Real-time inventory deduction + profitability calculation.
/// </summary>
public class ServiceExecutionService : IServiceExecutionService
{
    private readonly ApplicationDbContext _context;

    public ServiceExecutionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceExecution> StartServiceAsync(
        Guid appointmentId,
        Guid appointmentLineId,
        string staffUserId,
        string staffName,
        DateTime startTime,
        CancellationToken cancellationToken = default)
    {
        var appointmentLine = await _context.Set<AppointmentLine>()
            .Include(al => al.Service)
                .ThenInclude(s => s.MaterialRecipe)
                    .ThenInclude(r => r.BeautyProduct)
            .FirstOrDefaultAsync(al => al.Id == appointmentLineId, cancellationToken)
            ?? throw new InvalidOperationException($"AppointmentLine {appointmentLineId} not found.");

        var execution = new ServiceExecution
        {
            Id                = Guid.NewGuid(),
            CompanyId         = appointmentLine.Service.CompanyId,
            AppointmentId     = appointmentId,
            AppointmentLineId = appointmentLineId,
            ServiceId         = appointmentLine.ServiceId,
            StaffUserId       = staffUserId,
            StaffName         = staffName,
            ExecutionStartTime = startTime,
            ExecutionEndTime   = startTime, // updated on complete
            ServicePrice       = appointmentLine.Price,
            Status             = ServiceExecutionStatus.InProgress,
            Materials = appointmentLine.Service.MaterialRecipe.Select(r => new ServiceExecutionMaterial
            {
                Id               = Guid.NewGuid(),
                CompanyId        = appointmentLine.Service.CompanyId,
                BeautyProductId  = r.BeautyProductId,
                BeautyProduct    = r.BeautyProduct,
                PlannedQuantity  = r.QuantityUsed,
                ActualQuantity   = r.QuantityUsed, // default = planned
                Unit             = r.Unit,
                UnitCost         = 0 // updated on completion from ItemBatch
            }).ToList()
        };

        await _context.Set<ServiceExecution>().AddAsync(execution, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return execution;
    }

    public async Task<ServiceExecutionResult> CompleteServiceAsync(
        Guid serviceExecutionId,
        DateTime endTime,
        List<MaterialConsumptionInput>? overrides = null,
        CancellationToken cancellationToken = default)
    {
        var execution = await _context.Set<ServiceExecution>()
            .Include(e => e.Materials)
                .ThenInclude(m => m.BeautyProduct)
            .Include(e => e.Service)
            .FirstOrDefaultAsync(e => e.Id == serviceExecutionId, cancellationToken)
            ?? throw new InvalidOperationException($"ServiceExecution {serviceExecutionId} not found.");

        if (execution.Status != ServiceExecutionStatus.InProgress)
            throw new InvalidOperationException($"ServiceExecution {serviceExecutionId} is not InProgress.");

        // Apply overrides if staff used different quantities
        if (overrides?.Any() == true)
        {
            foreach (var override_ in overrides)
            {
                var material = execution.Materials
                    .FirstOrDefault(m => m.BeautyProductId == override_.BeautyProductId);
                if (material != null)
                    material.ActualQuantity = override_.ActualQuantity;
            }
        }

        // Deduct inventory using FEFO
        foreach (var material in execution.Materials)
        {
            var inventoryTx = await DeductInventoryFEFOAsync(
                execution.CompanyId,
                material.BeautyProductId,
                material.ActualQuantity,
                execution.AppointmentId,
                cancellationToken);

            material.InventoryTransactionId = inventoryTx.Id;
            material.UnitCost = inventoryTx.UnitCost;
        }

        // Calculate total material cost
        execution.TotalMaterialCost = execution.Materials.Sum(m => m.ActualQuantity * m.UnitCost);
        execution.ExecutionEndTime  = endTime;
        execution.Status            = ServiceExecutionStatus.Completed;

        // Update cached material cost on service
        execution.Service.MaterialCostCached = execution.TotalMaterialCost;

        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceExecutionResult(
            execution.Id,
            execution.ServicePrice,
            execution.TotalMaterialCost,
            execution.GrossProfit,
            execution.GrossProfitPercent,
            execution.Materials.Select(m => new MaterialConsumptionDetail(
                m.BeautyProduct.ItemId.ToString(),
                m.PlannedQuantity,
                m.ActualQuantity,
                m.VarianceQuantity,
                m.UnitCost,
                m.ActualQuantity * m.UnitCost
            )).ToList()
        );
    }

    public async Task<ServiceProfitabilityDto> GetProfitabilityAsync(
        Guid companyId,
        Guid? serviceId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<ServiceExecution>()
            .Where(e => e.CompanyId == companyId
                     && e.Status == ServiceExecutionStatus.Completed
                     && e.ExecutionStartTime >= from
                     && e.ExecutionStartTime <= to);

        if (serviceId.HasValue)
            query = query.Where(e => e.ServiceId == serviceId.Value);

        var executions = await query
            .Include(e => e.Service)
            .ToListAsync(cancellationToken);

        var serviceName = executions.FirstOrDefault()?.Service?.Name ?? "All Services";

        return new ServiceProfitabilityDto(
            serviceId,
            serviceName,
            executions.Count,
            executions.Sum(e => e.ServicePrice),
            executions.Sum(e => e.TotalMaterialCost),
            executions.Sum(e => e.GrossProfit),
            executions.Count > 0
                ? Math.Round(executions.Average(e => e.GrossProfitPercent), 2)
                : 0
        );
    }

    // ── FEFO Inventory Deduction ───────────────────────────────────────────

    private async Task<InventoryTransaction> DeductInventoryFEFOAsync(
        Guid companyId,
        Guid productId,
        decimal quantity,
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        // FEFO: pick batch with earliest expiry date first
        var batch = await _context.ItemBatches
            .Where(b => b.ItemId == productId
                     && b.CurrentQuantity >= quantity
                     && (b.ExpirationDate == null || b.ExpirationDate > DateTime.UtcNow))
            .OrderBy(b => b.ExpirationDate ?? DateTime.MaxValue)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Insufficient stock for product {productId}. Required: {quantity}");

        // Deduct from batch
        batch.CurrentQuantity -= quantity;

        // Create inventory transaction
        var tx = new InventoryTransaction
        {
            Id              = Guid.NewGuid(),
            CompanyId       = companyId,
            ItemId          = productId,
            Type = TransactionType.AdjustmentOut,
            Quantity        = -quantity,
            UnitCost        = batch.InitialQuantity > 0
                ? batch.CurrentQuantity / batch.InitialQuantity
                : 0,
            TotalCost       = quantity * (batch.InitialQuantity > 0
                ? batch.CurrentQuantity / batch.InitialQuantity
                : 0),
            TransactionDate = DateTime.UtcNow,
            ReferenceId     = appointmentId,
            ReferenceType   = "Appointment",
            WarehouseId     = Guid.Empty, // default warehouse — resolved at runtime
            Notes           = $"Batch: {batch.BatchNumber}"
        };

        await _context.InventoryTransactions.AddAsync(tx, cancellationToken);

        return tx;
    }
}

