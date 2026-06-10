namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface ITenantOnboardingIdentityService
{
    Task<(Guid UserId, string Email)> CreateAdminUserAndRolesAsync(Guid tenantId, string email, string fullName, string password);
}
