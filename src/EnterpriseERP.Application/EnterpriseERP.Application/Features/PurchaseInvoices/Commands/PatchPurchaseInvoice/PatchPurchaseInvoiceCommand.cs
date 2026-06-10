using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.PurchaseInvoices.DTOs;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.SharedKernel.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseInvoices.Commands.PatchPurchaseInvoice;

// ─── Request ────────────────────────────────────────────────────────────────
/// <summary>
/// Phase 9 (§9.4.4): Partial update of a DRAFT or REJECTED invoice.
/// Returns 409 ERR_CONFLICT_INVALID_STATE if status ≠ DRAFT | REJECTED.
/// </summary>
public record PatchPurchaseInvoiceCommand(
    Guid Id,
    string? Narrative,
    int? PaymentTermsDays
) : IRequest<PurchaseInvoiceDto>;

// ─── Handler ─────────────────────────────────────────────────────────────────
public class PatchPurchaseInvoiceCommandHandler
    : IRequestHandler<PatchPurchaseInvoiceCommand, PurchaseInvoiceDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public PatchPurchaseInvoiceCommandHandler(IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<PurchaseInvoiceDto> Handle(
        PatchPurchaseInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Repository<PurchaseInvoice>()
                          .GetByIdAsync(request.Id)
                      ?? throw new KeyNotFoundException(
                          $"Invoice {request.Id} not found.");

        // §9.4.4 – Only DRAFT may be patched (Rejected not a valid status in this domain)
        if (invoice.Status is not PurchaseInvoiceStatus.Draft)
        {
            throw new ConflictException(
                $"Invoice {invoice.InvoiceNumber} is in state '{invoice.Status}' " +
                "and cannot be modified. Only DRAFT invoices may be patched.",
                "ERR_CONFLICT_INVALID_STATE");
        }

        // Apply partial update
        if (request.Narrative is not null)
            invoice.Notes = request.Narrative;

        _unitOfWork.Repository<PurchaseInvoice>().Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PurchaseInvoiceDto
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
            RemainingAmount = invoice.RemainingAmount,
            Status = invoice.Status.ToString(),
            Notes = invoice.Notes,
            Lines = new()
        };
    }
}
