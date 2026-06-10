using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.SaaS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnterpriseERP.Application.BackgroundServices;

public class BillingCycleJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BillingCycleJob> _logger;

    public BillingCycleJob(IServiceProvider serviceProvider, ILogger<BillingCycleJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BillingCycleJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                /*
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var tenantRepository = unitOfWork.Repository<Tenant>();
                var invoiceRepository = unitOfWork.Repository<TenantInvoice>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                // Get all tenants with expiring subscriptions (within 7 days)
                var expiringTenants = await tenantRepository.FindAsync(
                    t => t.Status == TenantStatus.Active 
                         && t.SubscriptionEndDate <= DateTime.UtcNow.AddDays(7),
                    stoppingToken);

                foreach (var tenant in expiringTenants)
                {
                    // Check if invoice already exists for this period
                    var existingInvoice = await invoiceRepository.FindAsync(
                        i => i.TenantId == tenant.Id 
                             && i.PeriodStart == tenant.SubscriptionEndDate.AddDays(1)
                             && i.PeriodEnd == tenant.SubscriptionEndDate.AddMonths(1),
                        stoppingToken);

                    if (!existingInvoice.Any())
                    {
                        // Create new invoice
                        var invoice = new TenantInvoice
                        {
                            TenantId = tenant.Id,
                            SubscriptionPlanId = tenant.SubscriptionPlanId,
                            PeriodStart = tenant.SubscriptionEndDate.AddDays(1),
                            PeriodEnd = tenant.SubscriptionEndDate.AddMonths(1),
                            Amount = tenant.SubscriptionPlan.MonthlyPrice,
                            Currency = tenant.DefaultCurrency,
                            Status = TenantInvoiceStatus.Pending,
                            PaymentMethod = PaymentMethod.Stripe,
                            Notes = $"Monthly subscription invoice for {tenant.Name}"
                        };

                        await invoiceRepository.AddAsync(invoice);
                        await unitOfWork.SaveChangesAsync(stoppingToken);

                        // Send notification email
                        // await emailService.SendInvoiceEmailAsync(tenant.AdminEmail, invoice);

                        _logger.LogInformation("Created invoice {InvoiceId} for tenant {TenantName}", invoice.Id, tenant.Name);
                    }
                }

                // Check for overdue invoices (more than 7 days past due)
                var overdueInvoices = await invoiceRepository.FindAsync(
                    i => i.Status == TenantInvoiceStatus.Pending 
                         && i.PeriodEnd < DateTime.UtcNow.AddDays(-7),
                    stoppingToken);

                foreach (var invoice in overdueInvoices)
                {
                    invoice.Status = TenantInvoiceStatus.Overdue;
                    await unitOfWork.SaveChangesAsync(stoppingToken);

                    // Suspend tenant if grace period exceeded
                    var tenant = await tenantRepository.GetByIdAsync(invoice.TenantId, stoppingToken);
                    if (tenant != null && tenant.Status == TenantStatus.Active)
                    {
                        tenant.Status = TenantStatus.Suspended;
                        await unitOfWork.SaveChangesAsync(stoppingToken);

                        // await emailService.SendSuspensionEmailAsync(tenant.AdminEmail, tenant.Name);

                        _logger.LogWarning("Suspended tenant {TenantName} due to overdue invoice {InvoiceId}", tenant.Name, invoice.Id);
                    }
                }
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BillingCycleJob");
            }

            // Run daily
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }

        _logger.LogInformation("BillingCycleJob stopped.");
    }
}
