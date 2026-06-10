namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface ITenantConnectionProvider
{
    Task<string?> GetConnectionStringAsync();
}
