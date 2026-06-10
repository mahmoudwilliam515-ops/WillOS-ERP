using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Manufacturing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Services;

public interface IBOMCostingService
{
    Task<decimal> CalculateTotalCostAsync(Guid bomId);
}

public class BOMCostingService : IBOMCostingService
{
    private readonly IUnitOfWork _unitOfWork;

    public BOMCostingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<decimal> CalculateTotalCostAsync(Guid bomId)
    {
        var bom = await _unitOfWork.Repository<BillOfMaterials>().GetByIdAsync(bomId);
        if (bom == null) return 0;

        decimal materialCost = 0;
        decimal laborCost = 0;

        // 1. Material Costs (Recursive)
        foreach (var line in bom.Lines)
        {
            if (line.SubBOMId.HasValue)
            {
                materialCost += (await CalculateTotalCostAsync(line.SubBOMId.Value)) * line.Quantity;
            }
            else
            {
                var rawMaterial = await _unitOfWork.Repository<RawMaterial>().GetByIdAsync(line.RawMaterialId);
                materialCost += (rawMaterial?.CurrentCost ?? 0) * line.Quantity * (1 + (line.ScrapFactor / 100m));
            }
        }

        // 2. Labor & Overhead Costs (from Routing)
        var routing = (await _unitOfWork.Repository<Routing>().FindAsync(r => r.ProductId == bom.ProductId)).FirstOrDefault();
        if (routing != null)
        {
            foreach (var step in routing.Steps)
            {
                var wc = await _unitOfWork.Repository<WorkCenter>().GetByIdAsync(step.WorkCenterId);
                if (wc != null)
                {
                    var totalTimeHours = (step.SetupTimeMinutes + step.RunTimeMinutesPerUnit) / 60m;
                    laborCost += totalTimeHours * (wc.HourlyRate + wc.OverheadRate) / wc.EfficiencyFactor;
                }
            }
        }

        return materialCost + laborCost;
    }
}
