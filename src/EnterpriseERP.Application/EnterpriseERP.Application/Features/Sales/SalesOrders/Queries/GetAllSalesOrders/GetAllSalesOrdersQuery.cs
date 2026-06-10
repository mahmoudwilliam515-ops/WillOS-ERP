using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Sales.SalesOrders.Queries.GetAllSalesOrders;

public record GetAllSalesOrdersQuery(
    int Page = 1, 
    int PageSize = 20, 
    string? SearchTerm = null, 
    EnterpriseERP.Domain.Sales.SalesOrderStatus? Status = null,
    Guid? CustomerId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<GetAllSalesOrdersResult>;

public class GetAllSalesOrdersResult
{
    public List<SalesOrderDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class SalesOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class GetAllSalesOrdersQueryHandler : IRequestHandler<GetAllSalesOrdersQuery, GetAllSalesOrdersResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSalesOrdersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetAllSalesOrdersResult> Handle(GetAllSalesOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<SalesOrder>().Query()
            .Include(o => o.Customer)
            .AsQueryable();

        // ── Apply Filters ───────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(o => o.OrderNumber.ToLower().Contains(term) 
                                 || (o.Customer != null && o.Customer.Name.ToLower().Contains(term)));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Status.Value);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == request.CustomerId.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= request.ToDate.Value);
        }

        query = query.OrderByDescending(o => o.OrderDate);

        var total = await query.CountAsync(cancellationToken);
        var paged = await query.Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new GetAllSalesOrdersResult
        {
            TotalCount = total,
            Items = paged.Select(o => new SalesOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.Name ?? "N/A",
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString()
            }).ToList()
        };
    }
}
