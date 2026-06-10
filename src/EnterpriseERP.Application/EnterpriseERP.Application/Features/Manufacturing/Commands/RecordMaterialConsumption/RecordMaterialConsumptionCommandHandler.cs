using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.RecordMaterialConsumption;

public class RecordMaterialConsumptionCommandHandler : IRequestHandler<RecordMaterialConsumptionCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountingPostingService _accountingPostingService;

    public RecordMaterialConsumptionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAccountingPostingService accountingPostingService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _accountingPostingService = accountingPostingService;
    }

    public async Task<Result<bool>> Handle(RecordMaterialConsumptionCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var order = await _unitOfWork.Repository<ProductionOrder>()
                .Query()
                .Include(o => o.Materials)
                .FirstOrDefaultAsync(o => o.Id == request.ProductionOrderId, cancellationToken);

            if (order == null)
                return Result.Failure<bool>(new Error("ProductionOrder.NotFound", "Production order not found."));

            var userId = _currentUserService.UserId ?? "System";
            
            // 1. Domain Logic
            order.ConsumeMaterial(request.MaterialId, request.Quantity, userId);
            
            // Calculate cost for this consumption (simplified for now)
            var material = order.Materials.First(m => m.Id == request.MaterialId);
            var cost = request.Quantity * 100; // Placeholder for actual inventory cost

            // 2. Accounting Posting (Inventory to WIP)
            await _accountingPostingService.PostMaterialIssueAsync(order, cost, cancellationToken);

            _unitOfWork.Repository<ProductionOrder>().Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync();
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Failure<bool>(new Error("Manufacturing.Error", ex.Message));
        }
    }
}
