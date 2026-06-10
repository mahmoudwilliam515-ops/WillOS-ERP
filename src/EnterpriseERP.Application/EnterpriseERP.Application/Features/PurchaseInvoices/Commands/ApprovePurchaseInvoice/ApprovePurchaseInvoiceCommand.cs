using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Workflow;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseInvoices.Commands.ApprovePurchaseInvoice;

public record ApprovePurchaseInvoiceCommand(Guid InvoiceId) : IRequest<bool>;

public class ApprovePurchaseInvoiceCommandHandler : IRequestHandler<ApprovePurchaseInvoiceCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IInventoryPostingService _inventoryPostingService;
    private readonly IProjectCostService _projectCostService;
    private readonly IWorkflowService _workflowService;
    private readonly IPurchaseMatchingService _purchaseMatchingService;

    public ApprovePurchaseInvoiceCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountingPostingService accountingPostingService,
        IInventoryPostingService inventoryPostingService,
        IProjectCostService projectCostService,
        IWorkflowService workflowService,
        IPurchaseMatchingService purchaseMatchingService)
    {
        _unitOfWork = unitOfWork;
        _accountingPostingService = accountingPostingService;
        _inventoryPostingService = inventoryPostingService;
        _projectCostService = projectCostService;
        _workflowService = workflowService;
        _purchaseMatchingService = purchaseMatchingService;
    }

    public async Task<bool> Handle(ApprovePurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var invoice = await _unitOfWork.Repository<PurchaseInvoice>().GetByIdAsync(request.InvoiceId);

            if (invoice == null)
                throw new PurchasingDomainException($"Purchase invoice with id {request.InvoiceId} not found");

            // 0. Unified Workflow Check
            var isApproved = await _workflowService.IsFullyApprovedAsync(WorkflowDocumentType.PurchaseInvoice, invoice.Id, cancellationToken);
            if (!isApproved)
                throw new PurchasingDomainException("Purchase invoice is not fully approved in the workflow.");

            if (invoice.Status == PurchaseInvoiceStatus.Approved)
                throw new PurchasingDomainException("Invoice is already approved.");

            // Load invoice lines
            var lines = await _unitOfWork.Repository<PurchaseInvoiceLine>().FindAsync(x => x.PurchaseInvoiceId == invoice.Id);
            invoice.Lines = lines?.ToList() ?? new List<PurchaseInvoiceLine>();

            var (matchSuccess, matchMessage, matchStatus) = await _purchaseMatchingService.MatchInvoiceAsync(invoice);
            invoice.Status = matchStatus;
            invoice.Notes = AppendSystemNote(invoice.Notes, matchSuccess
                ? "P2P matching successful."
                : "P2P match failed: " + matchMessage);

            if (!matchSuccess)
            {
                _unitOfWork.Repository<PurchaseInvoice>().Update(invoice);
                await _unitOfWork.CommitTransactionAsync();
                return false;
            }

            // 1. Change Status
            invoice.Status = PurchaseInvoiceStatus.Approved;
            _unitOfWork.Repository<PurchaseInvoice>().Update(invoice);

            // 1.1 Project Costing Integration
            foreach (var line in invoice.Lines)
            {
                if (line.ProjectTaskId.HasValue)
                {
                    // If it was linked to a PO, reduce commitment
                    if (line.PurchaseOrderLineId.HasValue)
                    {
                        var poLine = await _unitOfWork.Repository<PurchaseOrderLine>().GetByIdAsync(line.PurchaseOrderLineId.Value);
                        if (poLine != null)
                        {
                            await _projectCostService.UpdateCommitmentAsync(line.ProjectTaskId.Value, poLine.LineTotal, false, cancellationToken);
                        }
                    }

                    // Add to Actual Cost (Material)
                    await _projectCostService.UpdateActualCostAsync(line.ProjectTaskId.Value, line.LineTotal, "material", cancellationToken);
                }
            }

            // 2. Generate Inventory Transactions via Domain Service
            var inventoryTxs = await _inventoryPostingService.PostPurchaseInvoiceAsync(invoice, cancellationToken);
            foreach (var tx in inventoryTxs)
            {
                await _unitOfWork.Repository<InventoryTransaction>().AddAsync(tx);
            }

            // 3. Generate Double-Entry Journal via Domain Service
            var journalEntry = await _accountingPostingService.PostPurchaseInvoiceAsync(invoice, cancellationToken);
            await _unitOfWork.Repository<JournalEntry>().AddAsync(journalEntry);
            foreach (var line in journalEntry.Lines)
                await _unitOfWork.Repository<JournalEntryLine>().AddAsync(line);

            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }


    private static string AppendSystemNote(string currentNotes, string note)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var systemNote = $"[{timestamp} UTC] {note}";

        return string.IsNullOrWhiteSpace(currentNotes)
            ? systemNote
            : $"{currentNotes.Trim()}{Environment.NewLine}{systemNote}";
    }
}
