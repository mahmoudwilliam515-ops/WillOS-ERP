using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Application.Features.FixedAssets.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.FixedAssets.Queries.GetFixedAssets;

public class GetFixedAssetsQuery : IRequest<PagedResult<FixedAssetDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? SearchTerm { get; set; }
}
