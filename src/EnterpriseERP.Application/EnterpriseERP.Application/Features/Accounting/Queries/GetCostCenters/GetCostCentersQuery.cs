using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Application.Features.Accounting.Queries.GetCostCenters;

public record GetCostCentersQuery : IRequest<Result<List<CostCenterDto>>>
{
    public bool? IsActive { get; init; }
}

public record CostCenterDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Guid? ParentId { get; init; }
}
