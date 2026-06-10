using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Infrastructure.Data;
using EnterpriseERP.Infrastructure.Identity;
using EnterpriseERP.Infrastructure.Repositories;
using EnterpriseERP.Application.Configuration;
using EnterpriseERP.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                var dbName = configuration.GetValue<string>("InMemoryDatabaseName") ?? "IntegrationTestsDb";
                options.UseInMemoryDatabase(dbName);
            }
            else
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            }
        });
        services.AddScoped<EnterpriseERP.Application.Common.Interfaces.IAppDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        // ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Domain Events Dispatcher
        services.AddScoped<EnterpriseERP.SharedKernel.DomainEvents.IDomainEventDispatcher, EnterpriseERP.SharedKernel.DomainEvents.DomainEventDispatcher>();

        // Services
        services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();
        services.AddScoped<ITenantConnectionProvider, TenantConnectionProvider>();
        services.AddScoped<IAIGovernanceService, AIGovernanceService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAIPredictiveService, AIPredictiveService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITenantOnboardingIdentityService, TenantOnboardingIdentityService>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<ILegacyDataMigrationService, LegacyDataMigrationService>();
        services.AddScoped<EnterpriseERP.Application.Features.Inventory.Services.IInventoryValuationService,
            EnterpriseERP.Application.Features.Inventory.Services.InventoryValuationService>();
        services.AddScoped<EnterpriseERP.Domain.Services.IFIFOValuationService, EnterpriseERP.Domain.Services.FIFOValuationService>();
        services.AddTransient<IEmailService, SmtpEmailService>();
        services.AddScoped<IPartyBalanceService, PartyBalanceService>();
        services.AddScoped<IAccountingPostingService, EnterpriseERP.Application.Services.AccountingPostingService>();
        services.AddScoped<IInventoryPostingService, EnterpriseERP.Application.Services.InventoryPostingService>();
        services.AddScoped<IPurchaseMatchingService, PurchaseMatchingService>();
        services.AddScoped<EnterpriseERP.Application.Services.IBOMCostingService, EnterpriseERP.Application.Services.BOMCostingService>();
        services.AddScoped<EnterpriseERP.Application.Services.IFixedAssetService, EnterpriseERP.Application.Services.FixedAssetService>();
        services.AddScoped<IInventoryReservationService, InventoryReservationService>();
        services.AddScoped<ISalesFulfillmentService, SalesFulfillmentService>();
        services.AddScoped<IIso20022Service, Iso20022Service>();
        services.AddScoped<IConsolidationService, ConsolidationService>();
        services.AddScoped<IProjectCostService, ProjectCostService>();
        services.AddScoped<IMrpService, MrpService>();
        services.AddScoped<IProductionSchedulerService, ProductionSchedulerService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IEInvoicingService, EInvoicingService>();
        services.Configure<EInvoicingOptions>(configuration.GetSection(EInvoicingOptions.SectionName));
        services.AddHttpClient("EInvoicing");

        // Dynamic Permissions Authorization
        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, EnterpriseERP.Infrastructure.Identity.Authorization.PermissionPolicyProvider>();
        services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, EnterpriseERP.Infrastructure.Identity.Authorization.PermissionAuthorizationHandler>();

        return services;
    }
}
