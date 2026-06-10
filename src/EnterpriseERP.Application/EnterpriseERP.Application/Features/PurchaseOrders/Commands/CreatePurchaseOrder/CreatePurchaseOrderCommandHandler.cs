using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Workflow;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkflowService _workflowService;
    private readonly ICurrentUserService _currentUserService;

    public CreatePurchaseOrderCommandHandler(IUnitOfWork unitOfWork, IWorkflowService workflowService, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _workflowService = workflowService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var companyId = _currentUserService.CompanyId;
        if (companyId == null) return Result.Failure<Guid>(new Error("Company.Required", "Company context is required."));

        var lines = new List<PurchaseOrderLine>();
        decimal subTotal = 0;
        decimal taxAmount = 0;

        foreach (var lineRequest in request.Lines)
        {
            var lineSubTotal = Math.Round(lineRequest.Quantity * lineRequest.UnitCost, 4);
            var lineTax = Math.Round(lineSubTotal * (lineRequest.TaxPercent / 100m), 4);
            var lineTotal = lineSubTotal + lineTax;

            lines.Add(new PurchaseOrderLine
            {
                Id = Guid.NewGuid(),
                ItemId = lineRequest.ItemId,
                ProjectTaskId = lineRequest.ProjectTaskId,
                Quantity = lineRequest.Quantity,
                UnitCost = lineRequest.UnitCost,
                TaxPercent = lineRequest.TaxPercent,
                TaxAmount = lineTax,
                LineTotal = lineTotal,
                Notes = lineRequest.Notes
            });

            subTotal += lineSubTotal;
            taxAmount += lineTax;
        }

        var order = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}",
            OrderDate = request.OrderDate,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            SupplierId = request.SupplierId,
            BranchId = request.BranchId,
            WarehouseId = request.WarehouseId,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            TotalAmount = subTotal + taxAmount,
            Notes = request.Notes,
            Status = PurchaseOrderStatus.Draft,
            Lines = lines
        };

        foreach (var line in lines)
        {
            line.PurchaseOrderId = order.Id;
        }

        await _unitOfWork.Repository<PurchaseOrder>().AddAsync(order);
        
        // 3. Submit for Approval
        await _workflowService.SubmitForApprovalAsync(
            WorkflowDocumentType.PurchaseOrder, 
            order.Id, 
            order.OrderNumber, 
            order.TotalAmount, 
            _currentUserService.UserId ?? "System", 
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(order.Id);
    }
}
