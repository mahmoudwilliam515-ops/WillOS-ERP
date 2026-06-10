using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Features.Identity.DTOs;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly IAuthorizationService _authorizationService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        ITokenService tokenService,
        IConfiguration configuration,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _configuration = configuration;
        _authorizationService = authorizationService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user == null || !user.IsActive)
            return Unauthorized(new { Success = false, Message = "Invalid credentials.", Errors = new[] { "Email or password is incorrect." } });

        var result = await _signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Unauthorized(new { Success = false, Message = "Invalid credentials.", Errors = new[] { "Email or password is incorrect." } });

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var authResponse = await _tokenService.GenerateTokenAsync(user.Id, user.Email!, user.FullName, user.BranchId, user.TenantId);

        return Ok(new { Success = true, Message = "Login successful", Data = authResponse, Errors = Array.Empty<string>() });
    }

    /// <summary>
    /// Registration is disabled in Production unless Auth:AllowPublicRegistration is true.
    /// When disabled, callers must be authenticated and hold Users.Create permission.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var allowPublicRegistration = _configuration.GetValue<bool>("Auth:AllowPublicRegistration");

        if (!allowPublicRegistration)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Public registration is disabled.",
                    Errors = new[] { "Authenticate as an administrator or enable Auth:AllowPublicRegistration." }
                });
            }

            var authorized = await _authorizationService.AuthorizeAsync(
                User,
                null,
                $"Permission:{Permissions.UsersCreate}");

            if (!authorized.Succeeded)
            {
                return Forbid();
            }
        }

        if (command.Password != command.ConfirmPassword)
            return BadRequest(new { Success = false, Message = "Passwords do not match.", Errors = new[] { "ConfirmPassword mismatch." } });

        if (await _userManager.FindByEmailAsync(command.Email) != null)
            return Conflict(new { Success = false, Message = "Email already registered.", Errors = new[] { "A user with this email already exists." } });

        var user = new ApplicationUser
        {
            FullName = command.FullName,
            Email = command.Email,
            UserName = command.Email,
            BranchId = command.BranchId,
            TenantId = command.TenantId,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
            return BadRequest(new { Success = false, Message = "Registration failed.", Errors = result.Errors.Select(e => e.Description).ToList() });

        if (!await _roleManager.RoleExistsAsync("Staff"))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = "Staff" });
        }
        await _userManager.AddToRoleAsync(user, "Staff");

        var authResponse = await _tokenService.GenerateTokenAsync(user.Id, user.Email!, user.FullName, user.BranchId, user.TenantId);

        return Ok(new { Success = true, Message = "Registration successful", Data = authResponse, Errors = Array.Empty<string>() });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        if (command == null || string.IsNullOrWhiteSpace(command.AccessToken) || string.IsNullOrWhiteSpace(command.RefreshToken))
            return BadRequest(new { Success = false, Message = "Invalid client request", Errors = new[] { "Missing tokens." } });

        try
        {
            var authResponse = await _tokenService.RefreshTokenAsync(command.AccessToken, command.RefreshToken);
            return Ok(new { Success = true, Message = "Token refreshed successfully", Data = authResponse, Errors = Array.Empty<string>() });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { Success = false, Message = ex.Message, Errors = new[] { ex.Message } });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Success = false, Message = "Invalid token", Errors = new[] { ex.Message } });
        }
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenCommand command)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user == null)
            return BadRequest(new { Success = false, Message = "Invalid user request." });

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = DateTime.MinValue;
        await _userManager.UpdateAsync(user);

        return Ok(new { Success = true, Message = "Token revoked successfully." });
    }
}

public class RevokeTokenCommand
{
    public string Email { get; set; } = string.Empty;
}
