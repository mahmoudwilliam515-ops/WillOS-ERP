using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Manufacturing.DTOs;
using EnterpriseERP.Domain.Entities.Manufacturing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Manufacturing.Queries.GetBillOfMaterials;

public class GetBillOfMaterialsQueryHandler : IRequestHandler<GetBillOfMaterialsQuery, List<BillOfMaterialsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBillOfMaterialsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<BillOfMaterialsDto>> Handle(GetBillOfMaterialsQuery request, CancellationToken cancellationToken)
    {
        var boms = await _unitOfWork.Repository<BillOfMaterials>().Query()
            .Include(b => b.Product)
            .Include(b => b.Lines)
                .ThenInclude(l => l.RawMaterial)
            .Include(b => b.Stages)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return boms.Select(b => new BillOfMaterialsDto
        {
            Id = b.Id,
            ProductId = b.ProductId,
            ProductName = b.Product.NameEn,
            Name = b.Name,
            Description = b.Description,
            Version = b.Version,
            IsDefault = b.IsDefault,
            IsActive = b.Status == BOMStatus.Active,
            LaborCostPerHour = b.LaborCostPerHour,
            MachineOverheadPerHour = b.MachineOverheadPerHour,
            TotalEstimatedCost = b.TotalEstimatedCost,
            Lines = b.Lines.Select(l => new BillOfMaterialsLineDto
            {
                Id = l.Id,
                MaterialId = l.RawMaterialId,
                MaterialName = l.RawMaterial.Name,
                Quantity = l.Quantity,
                Unit = l.Unit,
                ScrapFactor = l.ScrapFactor
            }).ToList(),
            Stages = b.Stages.Select(s => new ProductionStageDto
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
        }).ToList();
    }
}
