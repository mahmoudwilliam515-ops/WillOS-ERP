using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Manufacturing.DTOs;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Manufacturing;
using MediatR;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.CreateBillOfMaterials;

public class CreateBillOfMaterialsCommandHandler : IRequestHandler<CreateBillOfMaterialsCommand, BillOfMaterialsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBillOfMaterialsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BillOfMaterialsDto> Handle(CreateBillOfMaterialsCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify Product Exists
        var product = await _unitOfWork.Repository<Item>().GetByIdAsync(request.ProductId);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found.");

        // 2. If this is default, reset others for same product
        if (request.IsDefault)
        {
            var existingBoms = await _unitOfWork.Repository<BillOfMaterials>()
                .FindAsync(b => b.ProductId == request.ProductId && b.IsDefault);
            foreach (var bom in existingBoms)
            {
                bom.IsDefault = false;
                _unitOfWork.Repository<BillOfMaterials>().Update(bom);
            }
        }

        // 3. Calculate Total Estimated Cost
        var totalMaterialCost = request.Lines.Sum(l => l.Quantity * l.UnitCost);
        var totalStageCost = request.Stages.Sum(s => s.EstimatedHours * s.CostPerHour);
        var totalEstimatedCost = totalMaterialCost + totalStageCost;

        // 4. Create BOM
        var billOfMaterials = new BillOfMaterials
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Name = request.Name,
            Description = request.Description,
            Version = request.Version,
            IsDefault = request.IsDefault,
            Status = BOMStatus.Active,
            LaborCostPerHour = request.LaborCostPerHour,
            MachineOverheadPerHour = request.MachineOverheadPerHour,
            TotalEstimatedCost = totalEstimatedCost,
            Lines = request.Lines.Select(l => new BillOfMaterialsLine
            {
                Id = Guid.NewGuid(),
                RawMaterialId = l.MaterialId,
                Quantity = l.Quantity,
                Unit = "",
                ScrapFactor = 0
            }).ToList(),
            Stages = request.Stages.Select(s => new ProductionStage
            {
                Id = Guid.NewGuid(),
                Name = s.Name,
                Sequence = s.SequenceOrder,
                EstimatedHours = s.EstimatedHours,
                CostPerHour = s.CostPerHour,
                WorkCenterId = s.WorkCenterId,
                Description = s.Description
            }).ToList()
        };

        await _unitOfWork.Repository<BillOfMaterials>().AddAsync(billOfMaterials);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Map to DTO
        return new BillOfMaterialsDto
        {
            Id = billOfMaterials.Id,
            ProductId = billOfMaterials.ProductId,
            ProductName = product.NameEn,
            Name = billOfMaterials.Name,
            Description = billOfMaterials.Description,
            Version = billOfMaterials.Version,
            IsDefault = billOfMaterials.IsDefault,
            IsActive = billOfMaterials.Status == BOMStatus.Active,
            LaborCostPerHour = billOfMaterials.LaborCostPerHour,
            MachineOverheadPerHour = billOfMaterials.MachineOverheadPerHour,
            TotalEstimatedCost = billOfMaterials.TotalEstimatedCost,
            Lines = billOfMaterials.Lines.Select(l => new BillOfMaterialsLineDto
            {
                Id = l.Id,
                MaterialId = l.RawMaterialId,
                Quantity = l.Quantity,
                Unit = l.Unit,
                ScrapFactor = l.ScrapFactor
            }).ToList(),
            Stages = billOfMaterials.Stages.Select(s => new ProductionStageDto
            {
                Id = s.Id,
                Name = s.Name,
                Sequence = s.Sequence,
                EstimatedHours = s.EstimatedHours,
                CostPerHour = s.CostPerHour,
                WorkCenterId = s.WorkCenterId,
                WorkCenter = s.WorkCenter?.Name ?? string.Empty,
                Description = s.Description
            }).ToList()
        };
    }
}
