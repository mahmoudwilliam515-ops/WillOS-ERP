using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Workflow;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;

public record ApprovePurchaseOrderCommand(Guid OrderId) : IRequest<bool>;

public class ApprovePurchaseOrderCommandHandler : IRequestHandler<ApprovePurchaseOrderCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectCostService _projectCostService;
    private readonly IWorkflowService _workflowService;

    public ApprovePurchaseOrderCommandHandler(IUnitOfWork unitOfWork, IProjectCostService projectCostService, IWorkflowService workflowService)
    {
        _unitOfWork = unitOfWork;
        _projectCostService = projectCostService;
        _workflowService = workflowService;
    }

    public async Task<bool> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var order = await _unitOfWork.Repository<PurchaseOrder>().GetByIdAsync(request.OrderId);
            if (order == null) throw new PurchasingDomainException($"Purchase order {request.OrderId} not found.");

            // 0. Unified Workflow Check
            var isApproved = await _workflowService.IsFullyApprovedAsync(WorkflowDocumentType.PurchaseOrder, order.Id, cancellationToken);
            if (!isApproved)
                throw new PurchasingDomainException("Purchase order is not fully approved in the workflow.");

            if (order.Status != PurchaseOrderStatus.Draft)
                throw new PurchasingDomainException("Only draft purchase orders can be approved.");

            // 1. Change Status
            order.Status = PurchaseOrderStatus.Approved;
            _unitOfWork.Repository<PurchaseOrder>().Update(order);

            // 2. Record Commitments for Projects
            var lines = await _unitOfWork.Repository<PurchaseOrderLine>().FindAsync(l => l.PurchaseOrderId == order.Id);
            foreach (var line in lines)
            {
                if (line.ProjectTaskId.HasValue)
                {
                    await _projectCostService.UpdateCommitmentAsync(line.ProjectTaskId.Value, line.LineTotal, true, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
