using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Manufacturing.Queries.GetAllRawMaterials;

public class GetAllRawMaterialsQueryHandler : IRequestHandler<GetAllRawMaterialsQuery, PagedResult<RawMaterialDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllRawMaterialsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<RawMaterialDto>> Handle(GetAllRawMaterialsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Repository<RawMaterial>()
            .GetPagedAsync(request.PageNumber, request.PageSize,
                string.IsNullOrWhiteSpace(request.SearchTerm)
                    ? null
                    : r => r.Code.Contains(request.SearchTerm) || r.Name.Contains(request.SearchTerm));

        var dtos = items.Select(r => new RawMaterialDto
        {
            Id = r.Id,
            Code = r.Code,
            Name = r.Name,
            Unit = r.Unit,
            StockLevel = r.StockLevel,
            MinStock = r.MinStockLevel
        });

        return new PagedResult<RawMaterialDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
