using EnterpriseERP.Application.Common.Behaviors;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.SharedKernel.Behaviors;
using EnterpriseERP.Application.Common.Interfaces.Services;
using FluentValidation;
using EnterpriseERP.Application.Common.Interfaces.Services;
using MediatR;
using EnterpriseERP.Application.Common.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using EnterpriseERP.Application.Common.Interfaces.Services;
using System.Reflection;
using EnterpriseERP.Application.Common.Interfaces.Services;

namespace EnterpriseERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));

        // FluentValidation — scan all validators in Application assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // MediatR — scan all handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // ─────────────────────────────────────────────────────────────────
        // Pipeline Behaviors (applied in order: outer → inner)
        // 1. UnhandledExceptionBehavior  — catches & logs all unhandled exceptions
        // 2. LoggingBehavior             — logs request name on entry and exit
        // 3. AuditBehavior               — forensic audit trail for all Commands
        // 4. DomainExceptionBehavior     — logs DomainExceptions distinctly (re-throws)
        // 5. PerformanceBehavior         — warns on requests exceeding 500 ms
        // 6. ValidationBehavior          — validates input via FluentValidation
        // ─────────────────────────────────────────────────────────────────
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainExceptionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EnterpriseERP.SharedKernel.Behaviors.ValidationBehavior<,>));

        // ─────────────────────────────────────────────────────────────────
        // Domain / Application Services
        // ─────────────────────────────────────────────────────────────────
        services.AddScoped<
            EnterpriseERP.Application.Common.Interfaces.Services.IAccountingPostingService,
            EnterpriseERP.Application.Services.AccountingPostingService>();

        services.AddScoped<
            EnterpriseERP.Application.Common.Interfaces.Services.IInventoryPostingService,
            EnterpriseERP.Application.Services.InventoryPostingService>();

        return services;
    }
}
