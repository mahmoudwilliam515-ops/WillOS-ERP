using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.StartProductionOrder;

public class StartProductionOrderCommandHandler : IRequestHandler<StartProductionOrderCommand, Result<bool>>
{
    private readonly IGenericRepository<ProductionOrder> _orderRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public StartProductionOrderCommandHandler(
        IGenericRepository<ProductionOrder> orderRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _orderRepo = orderRepo;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(StartProductionOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepo.GetByIdAsync(request.ProductionOrderId);
        if (order == null)
            return Result.Failure<bool>(new Error("ProductionOrder.NotFound", "Production order not found."));

        var userId = _currentUserService.UserId ?? "System";
        
        // This will throw ManufacturingDomainException if validation fails
        order.Start(userId);

        _orderRepo.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
