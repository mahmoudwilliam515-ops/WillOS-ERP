using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Inventory.Commands.CreateInventoryReservation;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.SalesOrders.Commands.ApproveSalesOrder;

public record ApproveSalesOrderCommand(Guid SalesOrderId) : IRequest<Result<bool>>;

public class ApproveSalesOrderCommandHandler : IRequestHandler<ApproveSalesOrderCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public ApproveSalesOrderCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<Result<bool>> Handle(ApproveSalesOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var order = await _unitOfWork.Repository<SalesOrder>().Query()
                .Include(o => o.Lines)
                .FirstOrDefaultAsync(o => o.Id == request.SalesOrderId, cancellationToken);

            if (order == null) return Result.Failure<bool>(new Error("SalesOrder.NotFound", "Sales Order not found."));
            if (order.Status != SalesOrderStatus.Draft) return Result.Failure<bool>(new Error("SalesOrder.InvalidStatus", "Only draft orders can be approved."));

            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(order.CustomerId);
            if (customer == null) return Result.Failure<bool>(new Error("Customer.NotFound", "Customer not found."));

            // 1. Final Credit Check
            if (customer.CreditLimit > 0 && (customer.Balance + order.TotalAmount) > customer.CreditLimit)
            {
                return Result.Failure<bool>(new Error("Customer.CreditLimitExceeded", "Credit limit exceeded. Approval blocked."));
            }

            // 2. Inventory Reservation
            foreach (var line in order.Lines)
            {
                var reservationResult = await _mediator.Send(new CreateInventoryReservationCommand
                {
                    ItemId = line.ItemId,
                    WarehouseId = order.WarehouseId,
                    ReferenceId = order.Id,
                    ReferenceType = "SalesOrder",
                    ReferenceNumber = order.OrderNumber,
                    ReservedQuantity = line.Quantity,
                    ExpiresAt = DateTime.UtcNow.AddDays(7), // Reservation expires in 7 days
                    Notes = $"Reserved for SO {order.OrderNumber}"
                }, cancellationToken);

                if (!reservationResult.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Failure<bool>(reservationResult.Error);
                }
            }

            // 3. Update Order Status
            order.Status = SalesOrderStatus.Approved;
            _unitOfWork.Repository<SalesOrder>().Update(order);

            await _unitOfWork.CommitTransactionAsync();
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Failure<bool>(new Error("SalesOrder.ApprovalError", ex.Message));
        }
    }
}
