using System;
using System.Collections.Generic;
using EnterpriseERP.SharedKernel.DomainEvents;
using EnterpriseERP.Domain.Sales;

namespace EnterpriseERP.Domain.Sales.Events;

public record SalesOrderConfirmedEvent(
    Guid SalesOrderId,
    Guid CompanyId,
    Guid CustomerId,
    List<SalesOrderLine> Lines) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record SalesOrderCancelledEvent(
    Guid SalesOrderId,
    Guid CompanyId,
    List<SalesOrderLine> Lines) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record DeliveryNoteCompletedEvent(
    Guid DeliveryNoteId,
    Guid CompanyId,
    Guid SalesOrderId,
    List<DeliveryNoteLine> Lines) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record SalesReturnApprovedEvent(
    Guid SalesReturnId,
    Guid CompanyId,
    Guid CustomerId,
    Guid OriginalInvoiceId,
    DateTime ReturnDate,
    List<SalesReturnLine> Lines) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
