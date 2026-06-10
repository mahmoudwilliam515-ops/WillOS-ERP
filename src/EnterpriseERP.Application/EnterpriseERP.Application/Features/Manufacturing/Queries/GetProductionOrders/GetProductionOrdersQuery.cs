using EnterpriseERP.Application.Features.Manufacturing.DTOs;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System.Collections.Generic;

namespace EnterpriseERP.Application.Features.Manufacturing.Queries.GetProductionOrders;

public record GetProductionOrdersQuery : IRequest<Result<IEnumerable<ProductionOrderDto>>>
{
    public Guid? ProductId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
