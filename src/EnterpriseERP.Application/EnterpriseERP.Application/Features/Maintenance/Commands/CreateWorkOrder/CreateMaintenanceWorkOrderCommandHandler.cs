using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Maintenance;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Maintenance.Commands.CreateWorkOrder;

public class CreateMaintenanceWorkOrderCommandHandler : IRequestHandler<CreateMaintenanceWorkOrderCommand, Result<Guid>>
{
    private readonly IGenericRepository<MaintenanceWorkOrder> _workOrderRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMaintenanceWorkOrderCommandHandler(
        IGenericRepository<MaintenanceWorkOrder> workOrderRepo,
        IUnitOfWork unitOfWork)
    {
        _workOrderRepo = workOrderRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateMaintenanceWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = new MaintenanceWorkOrder
        {
            OrderNumber = $"MWO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            AssetId = request.AssetId,
            AssignedTechnicianId = request.AssignedTechnicianId,
            Description = request.Description,
            Status = MaintenanceWorkOrderStatus.Open,
            StartDate = DateTime.UtcNow
        };

        await _workOrderRepo.AddAsync(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(workOrder.Id);
    }
}
