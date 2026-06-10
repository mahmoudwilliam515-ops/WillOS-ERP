using EnterpriseERP.Application.Features.Inventory.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Inventory.Queries.GetStockBalance;

public class GetStockBalanceQuery : IRequest<IEnumerable<StockBalanceDto>>
{
    public Guid? WarehouseId { get; set; }
    public Guid? ItemId { get; set; }
    
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
