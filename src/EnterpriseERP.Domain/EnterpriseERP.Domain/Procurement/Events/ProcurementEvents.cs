using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.SharedKernel.DomainEvents;

namespace EnterpriseERP.Domain.Procurement.Events;

public record GRNApprovedEvent(
    Guid GRNId,
    Guid CompanyId,
    Guid PurchaseOrderId,
    Guid ApprovedByUserId,
    DateTime ReceiptDate,
    List<GoodsReceiptNoteLine> Lines) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PurchaseReturnApprovedEvent(
    Guid PurchaseReturnId,
    Guid CompanyId,
    Guid GoodsReceiptNoteId,
    Guid PurchaseOrderId,
    Guid SupplierId,
    Guid ApprovedByUserId,
    DateTime ReturnDate,
    List<PurchaseReturnLine> Lines) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record InvoiceMatchedEvent(
    Guid InvoiceId,
    Guid CompanyId,
    Guid PurchaseOrderId,
    Guid GRNId,
    decimal InvoiceAmount,
    DateTime MatchDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
