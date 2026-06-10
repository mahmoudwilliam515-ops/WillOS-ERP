using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Infrastructure.Services;

public class OrderNumberGenerator : IOrderNumberGenerator
{
    private readonly IAppDbContext _context;

    public OrderNumberGenerator(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync(string prefix, Guid companyId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        // Simple implementation: prefix + date + count
        var today = DateTime.UtcNow.Date;
        var count = await _context.SalesOrders
            .CountAsync(x => x.OrderDate == today && x.TenantId == tenantId, cancellationToken);
        
        return $"{prefix}-{today:yyyyMMdd}-{(count + 1):D4}";
    }
}
