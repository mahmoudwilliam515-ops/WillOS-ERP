using MediatR;

namespace EnterpriseERP.Application.Features.SaaS.Commands.OnboardTenant;

public record OnboardTenantCommand(
    string Name,
    string SubDomain,
    string AdminEmail,
    string AdminPassword,
    string AdminFullName,
    string CountryCode,
    Guid SubscriptionPlanId,
    string DefaultCurrency = "SAR",
    string DefaultLanguage = "ar",
    string TimeZone = "Asia/Riyadh"
) : IRequest<TenantOnboardingResult>;

public record TenantOnboardingResult(
    Guid TenantId,
    Guid AdminUserId,
    string AdminEmail,
    string SubDomain
);
