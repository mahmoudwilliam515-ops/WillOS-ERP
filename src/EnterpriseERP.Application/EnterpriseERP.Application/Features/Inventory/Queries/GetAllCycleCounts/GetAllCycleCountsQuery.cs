using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Domain.Entities.Inventory;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Inventory.Queries.GetAllCycleCounts;

public class GetAllCycleCountsQuery : IRequest<PagedResult<CycleCountDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SearchTerm { get; set; }
}

public class CycleCountDto
{
    public Guid Id { get; set; }
    public string CountNumber { get; set; } = string.Empty;
    public string CountDate { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CountedBy { get; set; } = string.Empty;
}

public class GetAllCycleCountsQueryHandler : IRequestHandler<GetAllCycleCountsQuery, PagedResult<CycleCountDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllCycleCountsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CycleCountDto>> Handle(GetAllCycleCountsQuery request, CancellationToken cancellationToken)
    {
        var counts = await _unitOfWork.Repository<CycleCount>().GetAllAsync();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            counts = counts.Where(c => c.CountNumber.ToLower().Contains(term)).ToList();
        }

        var warehouseIds = counts.Select(c => c.WarehouseId).Distinct().ToList();
        var warehouses = await _unitOfWork.Repository<Warehouse>().FindAsync(w => warehouseIds.Contains(w.Id));
        var warehouseDict = warehouses.ToDictionary(w => w.Id, w => w.Name);

        var totalCount = counts.Count();

        var paged = counts
            .OrderByDescending(c => c.CountDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CycleCountDto
            {
                Id = c.Id,
                CountNumber = c.CountNumber,
                CountDate = c.CountDate.ToString("yyyy-MM-dd"),
                WarehouseName = warehouseDict.TryGetValue(c.WarehouseId, out var wName) ? wName : "Unknown",
                Status = c.Status.ToString(),
                CountedBy = c.CreatedBy ?? "System"
            }).ToList();

        return new PagedResult<CycleCountDto>
        {
            Items = paged,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
