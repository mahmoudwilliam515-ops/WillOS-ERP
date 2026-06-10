using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IInventoryService
{
    Task IncreaseStockAsync(Guid itemId, Guid warehouseId, Guid companyId, decimal quantity, decimal unitCost, string reference, CancellationToken cancellationToken = default);
    Task<decimal> GetAvailableQuantityAsync(Guid itemId, Guid warehouseId, Guid companyId, CancellationToken cancellationToken = default);
}
