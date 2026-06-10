using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EnterpriseERP.Infrastructure.Services;

public class InventoryReservationService : IInventoryReservationService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryReservationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ReserveStockAsync(Guid itemId, Guid warehouseId, decimal quantity, Guid referenceId, string referenceType, string referenceNumber)
    {
        // 1. Check available stock (Stock - Reservations)
        var totalStock = await _unitOfWork.Repository<InventoryTransaction>().Query()
            .Where(t => t.ItemId == itemId && t.WarehouseId == warehouseId)
            .SumAsync(t => t.Quantity);

        var existingReservations = await _unitOfWork.Repository<InventoryReservation>().Query()
            .Where(r => r.ItemId == itemId && r.WarehouseId == warehouseId && r.Status == ReservationStatus.Active)
            .SumAsync(r => r.ReservedQuantity);

        var availableStock = totalStock - existingReservations;

        if (availableStock < quantity)
        {
            throw new Exception($"Insufficient stock for item {itemId}. Available: {availableStock}, Requested: {quantity}");
        }

        // 2. Create Reservation
        var reservation = new InventoryReservation
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            WarehouseId = warehouseId,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            ReferenceNumber = referenceNumber,
            ReservedQuantity = quantity,
            Status = ReservationStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddDays(7) // Default expiry
        };

        await _unitOfWork.Repository<InventoryReservation>().AddAsync(reservation);
        return reservation.Id;
    }

    public async Task CancelReservationAsync(Guid referenceId, string referenceType)
    {
        var reservations = await _unitOfWork.Repository<InventoryReservation>().FindAsync(r => r.ReferenceId == referenceId && r.ReferenceType == referenceType && r.Status == ReservationStatus.Active);
        foreach (var res in reservations)
        {
            res.Status = ReservationStatus.Cancelled;
            _unitOfWork.Repository<InventoryReservation>().Update(res);
        }
    }

    public async Task FulfillReservationAsync(Guid referenceId, string referenceType)
    {
        var reservations = await _unitOfWork.Repository<InventoryReservation>().FindAsync(r => r.ReferenceId == referenceId && r.ReferenceType == referenceType && r.Status == ReservationStatus.Active);
        foreach (var res in reservations)
        {
            res.Status = ReservationStatus.Fulfilled;
            _unitOfWork.Repository<InventoryReservation>().Update(res);
        }
    }
}
