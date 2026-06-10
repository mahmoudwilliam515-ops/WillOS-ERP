using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.SharedKernel.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseInvoices.Commands.CancelPurchaseInvoice;

// ─── Request ────────────────────────────────────────────────────────────────
/// <summary>
/// Phase 9 (§9.4.5): Soft-delete (cancel) a DRAFT invoice only.
/// Sets status = CANCELLED and deleted_at = now().
/// Approved or Posted invoices must be reversed via Credit Note.
/// </summary>
public record CancelPurchaseInvoiceCommand(Guid Id) : IRequest<CancelPurchaseInvoiceResult>;

public record CancelPurchaseInvoiceResult(
    Guid Id,
    string Status,
    DateTimeOffset DeletedAt);

// ─── Handler ─────────────────────────────────────────────────────────────────
public class CancelPurchaseInvoiceCommandHandler
    : IRequestHandler<CancelPurchaseInvoiceCommand, CancelPurchaseInvoiceResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelPurchaseInvoiceCommandHandler(IUnitOfWork unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<CancelPurchaseInvoiceResult> Handle(
        CancelPurchaseInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Repository<PurchaseInvoice>()
                          .GetByIdAsync(request.Id)
                      ?? throw new KeyNotFoundException(
                          $"Invoice {request.Id} not found.");

        // §9.4.5 — Only DRAFT may be soft-deleted
        if (invoice.Status != PurchaseInvoiceStatus.Draft)
        {
            throw new ConflictException(
                "Only DRAFT invoices can be cancelled. " +
                "Use Credit Note for approved/posted invoices.",
                "ERR_CONFLICT_CANNOT_DELETE");
        }

        invoice.Status = PurchaseInvoiceStatus.Cancelled;
        var deletedAt = DateTimeOffset.UtcNow;

        _unitOfWork.Repository<PurchaseInvoice>().Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CancelPurchaseInvoiceResult(invoice.Id, "CANCELLED", deletedAt);
    }
}
