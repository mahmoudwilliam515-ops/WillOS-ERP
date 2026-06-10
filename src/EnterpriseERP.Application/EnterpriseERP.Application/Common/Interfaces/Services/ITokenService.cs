using EnterpriseERP.Application.Features.Identity.DTOs;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

/// <summary>
/// Contract for JWT token generation and refresh.
/// Takes primitive user data to avoid circular dependency with Infrastructure.Identity.
/// </summary>
public interface ITokenService
{
    Task<AuthResponseDto> GenerateTokenAsync(string userId, string email, string fullName, Guid? branchId, Guid? tenantId);

    /// <summary>
    /// Validates the expired access token, verifies the refresh token,
    /// and issues a new token pair. Implements token rotation.
    /// </summary>
    Task<AuthResponseDto> RefreshTokenAsync(string expiredAccessToken, string refreshToken);
}
