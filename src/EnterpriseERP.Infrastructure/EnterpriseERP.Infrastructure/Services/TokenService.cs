using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Features.Identity.DTOs;
using EnterpriseERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly EnterpriseERP.Infrastructure.Data.ApplicationDbContext _dbContext;

    public TokenService(UserManager<ApplicationUser> userManager, IConfiguration configuration, EnterpriseERP.Infrastructure.Data.ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _configuration = configuration;
        _dbContext = dbContext;
    }

    public async Task<AuthResponseDto> GenerateTokenAsync(string userId, string email, string fullName, Guid? branchId, Guid? tenantId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException($"User {userId} not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("fullName", fullName),
            new("branchId", branchId?.ToString() ?? ""),
            new("TenantId", tenantId?.ToString() ?? ""),
            new("CompanyId", tenantId?.ToString() ?? "") // Map TenantId to CompanyId for context consistency
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(userClaims);
        
        // Fetch custom RolePermissions from the DB
        var rolePermissions = new List<string>();
        if (roles.Any())
        {
            var roleIds = await _dbContext.Roles
                .Where(r => roles.Contains(r.Name!))
                .Select(r => r.Id)
                .ToListAsync();

            rolePermissions = await _dbContext.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission)
                .ToListAsync();

            foreach (var permission in rolePermissions.Distinct())
            {
                claims.Add(new Claim("Permission", permission));
                claims.Add(new Claim("permission", permission)); // Add both casings to be safe
            }
        }

        var permissions = userClaims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .Concat(rolePermissions)
            .Distinct()
            .ToList();

        var jwtKey = _configuration["JwtSettings:Key"]
            ?? throw new InvalidOperationException("JWT Key is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60");
        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshExpiryDays"] ?? "7"));

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiry;
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            UserId = userId,
            FullName = fullName,
            Email = email,
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            TokenExpiry = expiry,
            RefreshTokenExpiry = refreshTokenExpiry,
            Roles = roles.ToList(),
            Permissions = permissions
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string expiredAccessToken, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(expiredAccessToken);
        if (principal == null)
            throw new InvalidOperationException("Invalid access token or refresh token.");

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId == null)
            throw new InvalidOperationException("Invalid token claims.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new InvalidOperationException("Invalid access token or refresh token.");

        // Generate new tokens
        return await GenerateTokenAsync(user.Id, user.Email!, user.FullName, user.BranchId, user.TenantId);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtKey = _configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _configuration["JwtSettings:Audience"],
            ValidateIssuer = true,
            ValidIssuer = _configuration["JwtSettings:Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = false // Here we are saying that we don't care about the token's expiration date
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
}
