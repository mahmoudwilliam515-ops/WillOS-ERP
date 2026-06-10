using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using System;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IInventoryReservationService
{
    Task<Guid> ReserveStockAsync(Guid itemId, Guid warehouseId, decimal quantity, Guid referenceId, string referenceType, string referenceNumber);
    Task CancelReservationAsync(Guid referenceId, string referenceType);
    Task FulfillReservationAsync(Guid referenceId, string referenceType);
}
