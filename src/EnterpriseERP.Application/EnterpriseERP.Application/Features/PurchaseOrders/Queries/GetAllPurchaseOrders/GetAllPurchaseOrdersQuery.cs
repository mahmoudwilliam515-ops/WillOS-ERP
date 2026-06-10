using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;

public class GetAllPurchaseOrdersQuery : IRequest<PagedResult<PurchaseOrderDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
}

public record PurchaseOrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public DateTime OrderDate { get; init; }
    public DateTime ExpectedDelivery { get; init; }
    public string SupplierName { get; init; } = string.Empty;
    public int LineCount { get; init; }
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
}
