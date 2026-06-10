using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Maintenance;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Maintenance.Commands.StartWorkOrder;

public class StartWorkOrderCommandHandler : IRequestHandler<StartWorkOrderCommand, Result<bool>>
{
    private readonly IGenericRepository<MaintenanceWorkOrder> _workOrderRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public StartWorkOrderCommandHandler(
        IGenericRepository<MaintenanceWorkOrder> workOrderRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _workOrderRepo = workOrderRepo;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(StartWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _workOrderRepo.GetByIdAsync(request.WorkOrderId);
        if (order == null)
            return Result.Failure<bool>(new Error("MaintenanceWorkOrder.NotFound", "Maintenance work order not found."));

        var userId = _currentUserService.UserId ?? "System";
        
        // This will throw MaintenanceDomainException if validation fails
        order.StartWork(userId);

        _workOrderRepo.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
