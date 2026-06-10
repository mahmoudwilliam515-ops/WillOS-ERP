using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Application.Features.FixedAssets.DTOs;
using EnterpriseERP.Domain.Entities.FixedAssets;
using MediatR;

namespace EnterpriseERP.Application.Features.FixedAssets.Queries.GetFixedAssets;

public class GetFixedAssetsQueryHandler : IRequestHandler<GetFixedAssetsQuery, PagedResult<FixedAssetDto>>
{
    private readonly IGenericRepository<FixedAsset> _repository;

    public GetFixedAssetsQueryHandler(IGenericRepository<FixedAsset> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<FixedAssetDto>> Handle(GetFixedAssetsQuery request, CancellationToken cancellationToken)
    {
        var pagedAssets = await _repository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            predicate: a => string.IsNullOrEmpty(request.SearchTerm) || a.NameEn.Contains(request.SearchTerm) || a.NameAr.Contains(request.SearchTerm) || a.Code.Contains(request.SearchTerm)
        );

        var dtos = pagedAssets.Items.OrderByDescending(a => a.CreatedAt).Select(a => new FixedAssetDto
        {
            Id = a.Id,
            Code = a.Code,
            NameAr = a.NameAr,
            NameEn = a.NameEn,
            Description = a.Description,
            PurchaseDate = a.PurchaseDate,
            PurchaseCost = a.PurchaseCost,
            SalvageValue = a.SalvageValue,
            UsefulLifeYears = a.UsefulLifeYears,
            DepreciationMethod = a.DepreciationMethod,
            AccumulatedDepreciation = a.AccumulatedDepreciation,
            NetBookValue = a.NetBookValue,
            IsActive = a.IsActive,
            BranchId = a.BranchId
        }).ToList();

        return new PagedResult<FixedAssetDto>
        {
            Items = dtos,
            TotalCount = pagedAssets.TotalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
