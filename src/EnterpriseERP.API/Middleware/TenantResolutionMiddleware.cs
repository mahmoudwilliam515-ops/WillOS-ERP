using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.SaaS;
using System.Security.Claims;

namespace EnterpriseERP.API.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ICurrentUserService currentUserService,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Guid? tenantId = null;

        // SECURITY FIX: Removed Header-based tenant resolution
        // TenantId MUST come from JWT claims only
        // Priority 1: JWT Claim 'tenant_id'
        if (tenantId == null && context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id");
            if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var jwtTenantId))
            {
                tenantId = jwtTenantId;
                _logger.LogDebug("Tenant ID extracted from JWT claim: {TenantId}", tenantId);
            }
        }

        // Priority 3: Subdomain (web clients)
        if (tenantId == null)
        {
            var host = context.Request.Host.Host;
            var subdomain = ExtractSubdomain(host);
            if (!string.IsNullOrEmpty(subdomain))
            {
                // TODO: Implement subdomain to tenant lookup
                // For now, this is a placeholder
                _logger.LogDebug("Subdomain extracted: {Subdomain}", subdomain);
            }
        }

        if (tenantId == null)
        {
            _logger.LogWarning("Tenant ID could not be resolved");
            context.Response.StatusCode = 401;
            return;
        }

        // Set tenant in context
        context.Items["TenantId"] = tenantId;
        
        await _next(context);
    }

    private string? ExtractSubdomain(string host)
    {
        if (string.IsNullOrEmpty(host))
            return null;

        var parts = host.Split('.');
        if (parts.Length > 2)
        {
            return parts[0];
        }

        return null;
    }
}

