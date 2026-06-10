using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Maintenance;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Maintenance.Commands.CompleteWorkOrder;

public class CompleteWorkOrderCommandHandler : IRequestHandler<CompleteWorkOrderCommand, Result<bool>>
{
    private readonly IGenericRepository<MaintenanceWorkOrder> _workOrderRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CompleteWorkOrderCommandHandler(
        IGenericRepository<MaintenanceWorkOrder> workOrderRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _workOrderRepo = workOrderRepo;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(CompleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _workOrderRepo.GetByIdAsync(request.WorkOrderId);
        if (order == null)
            return Result.Failure<bool>(new Error("MaintenanceWorkOrder.NotFound", "Maintenance work order not found."));

        var userId = _currentUserService.UserId ?? "System";
        
        // This will throw MaintenanceDomainException if validation fails
        order.Complete(request.ResolutionNotes, request.TotalPartsCost, request.TotalLaborCost, userId);

        _workOrderRepo.Update(order);
        
        // This saves the entity and dispatches the Domain Event (MaintenanceWorkOrderCompletedEvent)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
