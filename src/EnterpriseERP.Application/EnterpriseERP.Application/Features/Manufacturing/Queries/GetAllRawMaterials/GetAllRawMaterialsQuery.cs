using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Manufacturing.Queries.GetAllRawMaterials;

public class GetAllRawMaterialsQuery : IRequest<PagedResult<RawMaterialDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SearchTerm { get; set; }
}

public record RawMaterialDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public decimal StockLevel { get; init; }
    public decimal MinStock { get; init; }
}
