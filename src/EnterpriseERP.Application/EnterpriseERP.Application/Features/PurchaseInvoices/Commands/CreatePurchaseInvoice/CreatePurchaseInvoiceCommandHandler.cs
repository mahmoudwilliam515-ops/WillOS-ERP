using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Features.PurchaseInvoices.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Workflow;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseInvoices.Commands.CreatePurchaseInvoice;

public class CreatePurchaseInvoiceCommandHandler : IRequestHandler<CreatePurchaseInvoiceCommand, Result<PurchaseInvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IInventoryPostingService _inventoryPostingService;
    private readonly IPurchaseMatchingService _matchingService;
    private readonly IWorkflowService _workflowService;
    private readonly ICurrentUserService _currentUserService;

    public CreatePurchaseInvoiceCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountingPostingService accountingPostingService,
        IInventoryPostingService inventoryPostingService,
        IPurchaseMatchingService matchingService,
        IWorkflowService workflowService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _accountingPostingService = accountingPostingService;
        _inventoryPostingService = inventoryPostingService;
        _matchingService = matchingService;
        _workflowService = workflowService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PurchaseInvoiceDto>> Handle(CreatePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var lines = new List<PurchaseInvoiceLine>();
            decimal subTotal = 0, totalDiscount = 0, totalTax = 0;

            foreach (var lineReq in request.Lines)
            {
                var gross = lineReq.Quantity * lineReq.UnitCost;
                var discount = Math.Round(gross * (lineReq.DiscountPercent / 100m), 4);
                var netBeforeTax = gross - discount;
                var tax = Math.Round(netBeforeTax * (lineReq.TaxPercent / 100m), 4);
                var lineTotal = netBeforeTax + tax;

                lines.Add(new PurchaseInvoiceLine
                {
                    Id = Guid.NewGuid(),
                    ItemId = lineReq.ItemId,
                    PurchaseOrderLineId = lineReq.PurchaseOrderLineId,
                    GoodsReceiptLineId = lineReq.GoodsReceiptLineId,
                    ProjectTaskId = lineReq.ProjectTaskId,
                    Quantity = lineReq.Quantity,
                    UnitCost = lineReq.UnitCost,
                    DiscountPercent = lineReq.DiscountPercent,
                    DiscountAmount = discount,
                    TaxPercent = lineReq.TaxPercent,
                    TaxAmount = tax,
                    LineTotal = lineTotal,
                    Notes = lineReq.Notes
                });

                subTotal += gross;
                totalDiscount += discount;
                totalTax += tax;
            }

            var totalAmount = subTotal - totalDiscount + totalTax;
            var remaining = totalAmount - request.PaidAmount;
            var invoiceNumber = $"PI-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            var invoice = new PurchaseInvoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = invoiceNumber,
                SupplierInvoiceNumber = request.SupplierInvoiceNumber,
                InvoiceDate = request.InvoiceDate,
                SupplierId = request.SupplierId,
                PurchaseOrderId = request.PurchaseOrderId,
                BranchId = request.BranchId,
                WarehouseId = request.WarehouseId,
                SubTotal = subTotal,
                DiscountAmount = totalDiscount,
                TaxAmount = totalTax,
                TotalAmount = totalAmount,
                PaidAmount = request.PaidAmount,
                RemainingAmount = remaining,
                Notes = request.Notes,
                Status = PurchaseInvoiceStatus.Draft,
                Lines = lines
            };

            // Perform 3-way matching
            var matchingResult = await _matchingService.MatchInvoiceAsync(invoice);
            invoice.Status = matchingResult.ResultStatus;
            invoice.Notes += matchingResult.Message;

            await _unitOfWork.Repository<PurchaseInvoice>().AddAsync(invoice);

            // Submit for Workflow Approval
            await _workflowService.SubmitForApprovalAsync(
                WorkflowDocumentType.PurchaseInvoice,
                invoice.Id,
                invoice.InvoiceNumber,
                invoice.TotalAmount,
                _currentUserService.UserId ?? "System",
                cancellationToken);

            // If workflow already auto-approves and invoice is matched, mark it ready for payment.
            var isFullyApproved = await _workflowService.IsFullyApprovedAsync(WorkflowDocumentType.PurchaseInvoice, invoice.Id, cancellationToken);
            if (isFullyApproved && invoice.Status == PurchaseInvoiceStatus.Matched)
            {
                invoice.Status = PurchaseInvoiceStatus.ApprovedForPayment;
            }

            // Trigger Accounting Posting if Matched, Approved or ApprovedForPayment
            if (isFullyApproved && (invoice.Status == PurchaseInvoiceStatus.Matched ||
                                    invoice.Status == PurchaseInvoiceStatus.Approved ||
                                    invoice.Status == PurchaseInvoiceStatus.ApprovedForPayment))
            {
                await _accountingPostingService.PostPurchaseInvoiceAsync(invoice, cancellationToken);
            }

            await _unitOfWork.CommitTransactionAsync();

            var dto = new PurchaseInvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                SupplierInvoiceNumber = invoice.SupplierInvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                SupplierId = invoice.SupplierId,
                BranchId = invoice.BranchId,
                WarehouseId = invoice.WarehouseId,
                SubTotal = invoice.SubTotal,
                DiscountAmount = invoice.DiscountAmount,
                TaxAmount = invoice.TaxAmount,
                TotalAmount = invoice.TotalAmount,
                PaidAmount = invoice.PaidAmount,
                RemainingAmount = remaining,
                Status = invoice.Status.ToString(),
                Notes = invoice.Notes,
                Lines = lines.Select(l => new PurchaseInvoiceLineDto
                {
                    Id = l.Id,
                    ItemId = l.ItemId,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost,
                    DiscountPercent = l.DiscountPercent,
                    DiscountAmount = l.DiscountAmount,
                    TaxPercent = l.TaxPercent,
                    TaxAmount = l.TaxAmount,
                    LineTotal = l.LineTotal
                }).ToList()
            };

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Failure<PurchaseInvoiceDto>(new Error("PurchaseInvoice.CreateError", ex.Message));
        }
    }
}
