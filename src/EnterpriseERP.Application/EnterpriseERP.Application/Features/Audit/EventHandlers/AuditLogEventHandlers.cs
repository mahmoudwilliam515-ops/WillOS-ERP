using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Audit;
using EnterpriseERP.Application.Features.SalesInvoices.Events;
using EnterpriseERP.Application.Features.PurchaseInvoices.Events;
using EnterpriseERP.Application.Features.FixedAssets.Events;
using EnterpriseERP.Application.Features.HR.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseERP.Application.Features.Audit.EventHandlers;

public class AuditLogEventHandlers :
    INotificationHandler<SalesInvoiceCreatedEvent>,
    INotificationHandler<SalesInvoiceApprovedEvent>,
    INotificationHandler<PurchaseInvoiceCreatedEvent>,
    INotificationHandler<FixedAssetCreatedEvent>,
    INotificationHandler<PayrollProcessedEvent>
{
    private readonly IGenericRepository<AuditLog> _auditLogRepository;
    private readonly ILogger<AuditLogEventHandlers> _logger;

    public AuditLogEventHandlers(IGenericRepository<AuditLog> auditLogRepository, ILogger<AuditLogEventHandlers> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task Handle(SalesInvoiceCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: {DomainEvent} - Invoice {InvoiceId} created", notification.GetType().Name, notification.InvoiceId);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            EventType = notification.GetType().Name,
            EntityName = "SalesInvoice",
            EntityId = notification.InvoiceId,
            Action = "Created",
            Details = $"SalesInvoice {notification.InvoiceNumber} created for Customer {notification.CustomerId} with total {notification.TotalAmount}",
            OccurredOn = notification.OccurredOn
        };

        await _auditLogRepository.AddAsync(auditLog);
    }

    public async Task Handle(SalesInvoiceApprovedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: {DomainEvent} - Invoice {InvoiceId} approved", notification.GetType().Name, notification.InvoiceId);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            EventType = notification.GetType().Name,
            EntityName = "SalesInvoice",
            EntityId = notification.InvoiceId,
            Action = "Approved",
            Details = $"SalesInvoice {notification.InvoiceId} approved for Customer {notification.CustomerId} with total {notification.TotalAmount}",
            OccurredOn = notification.OccurredOn
        };

        await _auditLogRepository.AddAsync(auditLog);
    }

    public async Task Handle(PurchaseInvoiceCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: {DomainEvent} - PurchaseInvoice {InvoiceId} created", notification.GetType().Name, notification.InvoiceId);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            EventType = notification.GetType().Name,
            EntityName = "PurchaseInvoice",
            EntityId = notification.InvoiceId,
            Action = "Created",
            Details = $"PurchaseInvoice {notification.InvoiceId} created for Supplier {notification.SupplierId} with total {notification.TotalAmount}",
            OccurredOn = notification.OccurredOn
        };

        await _auditLogRepository.AddAsync(auditLog);
    }

    public async Task Handle(FixedAssetCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: {DomainEvent} - FixedAsset {AssetId} created", notification.GetType().Name, notification.AssetId);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            EventType = notification.GetType().Name,
            EntityName = "FixedAsset",
            EntityId = notification.AssetId,
            Action = "Created",
            Details = $"FixedAsset {notification.AssetName} created with PurchasePrice {notification.PurchaseValue}",
            OccurredOn = notification.OccurredOn
        };

        await _auditLogRepository.AddAsync(auditLog);
    }

    public async Task Handle(PayrollProcessedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain Event: {DomainEvent} - Payroll for Month {Month}, Year {Year} processed", notification.GetType().Name, notification.Month, notification.Year);

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            EventType = notification.GetType().Name,
            EntityName = "Payroll",
            EntityId = Guid.Empty,
            Action = "Processed",
            Details = $"Payroll for {notification.Month}/{notification.Year} processed. Employees: {notification.EmployeeCount}, Net: {notification.TotalNetSalaries}",
            OccurredOn = notification.OccurredOn
        };

        await _auditLogRepository.AddAsync(auditLog);
    }
}
