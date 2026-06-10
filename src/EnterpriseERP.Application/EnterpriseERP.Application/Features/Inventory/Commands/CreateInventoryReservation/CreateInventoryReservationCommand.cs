using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.Inventory.Commands.CreateInventoryReservation;

public class CreateInventoryReservationCommand : IRequest<Result<Guid>>
{
    public Guid ItemId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceType { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal ReservedQuantity { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? Notes { get; set; }
}

public class CreateInventoryReservationCommandHandler : IRequestHandler<CreateInventoryReservationCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateInventoryReservationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateInventoryReservationCommand request, CancellationToken cancellationToken)
    {
        if (request.ReservedQuantity <= 0)
            return Result.Failure<Guid>(new Error("Reservation.InvalidQuantity", "Reserved quantity must be greater than zero."));

        // Check available stock (total transactions - existing active reservations)
        var transactions = await _unitOfWork.Repository<InventoryTransaction>()
            .FindAsync(t => t.ItemId == request.ItemId && t.WarehouseId == request.WarehouseId);
        var stockBalance = transactions.Sum(t => t.Quantity);

        var activeReservations = await _unitOfWork.Repository<InventoryReservation>()
            .FindAsync(r => r.ItemId == request.ItemId &&
                            r.WarehouseId == request.WarehouseId &&
                            r.Status == ReservationStatus.Active);
        var reservedBalance = activeReservations.Sum(r => r.ReservedQuantity);

        var availableBalance = stockBalance - reservedBalance;

        if (request.ReservedQuantity > availableBalance)
            return Result.Failure<Guid>(new Error("Reservation.InsufficientStock", $"Insufficient stock. Available: {availableBalance}, Requested: {request.ReservedQuantity}."));

        var reservation = new InventoryReservation
        {
            Id = Guid.NewGuid(),
            ItemId = request.ItemId,
            WarehouseId = request.WarehouseId,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            ReferenceNumber = request.ReferenceNumber,
            ReservedQuantity = request.ReservedQuantity,
            Status = ReservationStatus.Active,
            ExpiresAt = request.ExpiresAt,
            Notes = request.Notes
        };

        await _unitOfWork.Repository<InventoryReservation>().AddAsync(reservation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(reservation.Id);
    }
}
