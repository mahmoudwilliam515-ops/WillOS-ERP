using EnterpriseERP.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace EnterpriseERP.Infrastructure.Services;

public class TenantConnectionProvider : ITenantConnectionProvider
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;
    // In a real scenario, you'd inject a TenantDbContext (Host) here to fetch from DB
    // For this implementation, we will assume a cache or fallback to DefaultConnection

    public TenantConnectionProvider(ICurrentUserService currentUserService, IConfiguration configuration)
    {
        _currentUserService = currentUserService;
        _configuration = configuration;
    }

    public Task<string?> GetConnectionStringAsync()
    {
        // 1. If no tenant, return default
        if (_currentUserService.TenantId == null || _currentUserService.TenantId == Guid.Empty)
        {
            return Task.FromResult(_configuration.GetConnectionString("DefaultConnection"));
        }

        // 2. Logic to find Tenant specific connection string
        // This is where we'd query the 'Tenants' table in the Master/Host DB.
        // For now, we return default, but the architecture is ready to be swapped.
        return Task.FromResult(_configuration.GetConnectionString("DefaultConnection"));
    }
}
