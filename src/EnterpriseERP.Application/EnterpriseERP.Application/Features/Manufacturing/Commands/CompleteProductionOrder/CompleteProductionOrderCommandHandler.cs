using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Domain.Entities.Quality;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Manufacturing.Commands.CompleteProductionOrder;

public class CompleteProductionOrderCommandHandler : IRequestHandler<CompleteProductionOrderCommand, Result<bool>>
{
    private readonly IGenericRepository<ProductionOrder> _orderRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IInventoryPostingService _inventoryPostingService;
    private readonly IAccountingPostingService _accountingPostingService;

    public CompleteProductionOrderCommandHandler(
        IGenericRepository<ProductionOrder> orderRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IInventoryPostingService inventoryPostingService,
        IAccountingPostingService accountingPostingService)
    {
        _orderRepo = orderRepo;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _inventoryPostingService = inventoryPostingService;
        _accountingPostingService = accountingPostingService;
    }

    public async Task<Result<bool>> Handle(CompleteProductionOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var order = await _unitOfWork.Repository<ProductionOrder>().GetByIdAsync(request.ProductionOrderId);
            if (order == null)
                return Result.Failure<bool>(new Error("ProductionOrder.NotFound", "Production order not found."));

            var userId = _currentUserService.UserId ?? "System";
            
            // 0. Quality Check Gate
            var product = await _unitOfWork.Repository<Item>().GetByIdAsync(order.ProductId);
            var inspections = await _unitOfWork.Repository<QualityInspection>()
                .FindAsync(i => i.ReferenceId == order.Id && i.Type == InspectionType.ProductionOutput);
            
            var latestInspection = inspections.OrderByDescending(i => i.CreatedAt).FirstOrDefault();

            if (product != null && product.IsQualityControlRequired)
            {
                if (latestInspection == null)
                {
                    throw new ManufacturingDomainException($"Quality Inspection is required for product {product.Name} before completing the production order.");
                }
                
                if (latestInspection.Status != InspectionStatus.Passed)
                {
                    throw new ManufacturingDomainException($"Cannot complete Production Order. Quality Inspection {latestInspection.InspectionNumber} is {latestInspection.Status}.");
                }
            }
            else if (latestInspection != null && latestInspection.Status == InspectionStatus.Pending)
            {
                // If an inspection was requested but not completed, even if not strictly required by product, we might want to block?
                // For now, let's keep it optional unless product requires it or inspection is failed.
                if (latestInspection.Status == InspectionStatus.Failed)
                {
                    throw new ManufacturingDomainException($"Cannot complete Production Order. Quality Inspection {latestInspection.InspectionNumber} failed.");
                }
            }

            // 1. Domain Logic Completion
            order.Complete(request.ProducedQuantity, request.TotalLaborCost, request.TotalMaterialCost, request.WarehouseId, userId);

            // 2. Inventory Posting (Real ERP movement)
            var inventoryTxns = await _inventoryPostingService.PostProductionCompletionAsync(order, cancellationToken);
            foreach (var txn in inventoryTxns)
            {
                await _unitOfWork.Repository<InventoryTransaction>().AddAsync(txn);
            }

            // 3. Financial Posting (WIP to Finished Goods)
            await _accountingPostingService.PostProductionOrderCompletionAsync(order, cancellationToken);

            await _unitOfWork.CommitTransactionAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Failure<bool>(new Error("ProductionOrder.CompletionFailed", ex.Message));
        }
    }
}
