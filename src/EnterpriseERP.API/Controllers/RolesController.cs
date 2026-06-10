using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Features.Roles.DTOs;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Infrastructure.Data;
using EnterpriseERP.Infrastructure.Identity;
using EnterpriseERP.Infrastructure.Identity.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public RolesController(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        ICurrentUserService currentUser)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    // ─── Roles CRUD ────────────────────────────────────────────────────────────

    /// <summary>الحصول على جميع الأدوار مع صلاحياتها</summary>
    [HttpGet]
    [HasPermission(Permissions.RolesManage)]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();

        var result = new List<RoleDto>();
        foreach (var role in roles)
        {
            var permissions = await _dbContext.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.Permission)
                .ToListAsync();

            result.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name!,
                Description = role.Description,
                Permissions = permissions
            });
        }

        return Ok(new { Success = true, Data = result, Errors = Array.Empty<string>() });
    }

    /// <summary>إنشاء دور جديد مع تعيين صلاحياته</summary>
    [HttpPost]
    [HasPermission(Permissions.RolesManage)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
    {
        if (await _roleManager.RoleExistsAsync(command.Name))
            return Conflict(new { Success = false, Message = $"Role '{command.Name}' already exists.", Errors = new[] { "Duplicate role name." } });

        var role = new ApplicationRole
        {
            Name = command.Name,
            Description = command.Description
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return BadRequest(new { Success = false, Message = "Failed to create role.", Errors = result.Errors.Select(e => e.Description) });

        // Assign permissions
        if (command.Permissions.Any())
        {
            var rolePermissions = command.Permissions.Select(p => new RolePermission
            {
                RoleId = role.Id,
                Permission = p,
                Module = p.Split('.').FirstOrDefault() ?? "General"
            });
            _dbContext.RolePermissions.AddRange(rolePermissions);
            await _dbContext.SaveChangesAsync();
        }

        return Ok(new { Success = true, Message = $"Role '{role.Name}' created successfully.", Data = new { role.Id, role.Name }, Errors = Array.Empty<string>() });
    }

    /// <summary>تحديث صلاحيات دور موجود (replace all)</summary>
    [HttpPut("{roleId}/permissions")]
    [HasPermission(Permissions.RolesManage)]
    public async Task<IActionResult> UpdateRolePermissions(string roleId, [FromBody] UpdateRolePermissionsCommand command)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return NotFound(new { Success = false, Message = "Role not found." });

        // Remove existing
        var existing = _dbContext.RolePermissions.Where(rp => rp.RoleId == roleId);
        _dbContext.RolePermissions.RemoveRange(existing);

        // Add new
        var newPermissions = command.Permissions.Select(p => new RolePermission
        {
            RoleId = roleId,
            Permission = p,
            Module = p.Split('.').FirstOrDefault() ?? "General"
        });
        _dbContext.RolePermissions.AddRange(newPermissions);
        await _dbContext.SaveChangesAsync();

        return Ok(new { Success = true, Message = $"Permissions updated for role '{role.Name}'.", Errors = Array.Empty<string>() });
    }

    /// <summary>حذف دور</summary>
    [HttpDelete("{roleId}")]
    [HasPermission(Permissions.RolesManage)]
    public async Task<IActionResult> DeleteRole(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return NotFound(new { Success = false, Message = "Role not found." });

        if (role.Name == "Admin")
            return BadRequest(new { Success = false, Message = "Cannot delete the Admin role." });

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return BadRequest(new { Success = false, Message = "Failed to delete role.", Errors = result.Errors.Select(e => e.Description) });

        return Ok(new { Success = true, Message = $"Role '{role.Name}' deleted successfully.", Errors = Array.Empty<string>() });
    }

    /// <summary>الحصول على قائمة جميع الصلاحيات المتاحة في النظام</summary>
    [HttpGet("available-permissions")]
    [HasPermission(Permissions.RolesManage)]
    public IActionResult GetAvailablePermissions()
    {
        var permissions = typeof(Permissions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(fi => new
            {
                Key = fi.Name,
                Value = (string)fi.GetRawConstantValue()!,
                Module = ((string)fi.GetRawConstantValue()!).Split('.').FirstOrDefault()
            })
            .GroupBy(p => p.Module)
            .Select(g => new { Module = g.Key, Permissions = g.Select(p => new { p.Key, p.Value }).ToList() })
            .ToList();

        return Ok(new { Success = true, Data = permissions, Errors = Array.Empty<string>() });
    }

    // ─── User Management ───────────────────────────────────────────────────────

    /// <summary>الحصول على جميع المستخدمين</summary>
    [HttpGet("/api/users")]
    [HasPermission(Permissions.UsersView)]
    public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var total = await _userManager.Users.CountAsync();

        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                IsActive = user.IsActive,
                BranchId = user.BranchId,
                Roles = roles.ToList(),
                LastLoginAt = user.LastLoginAt
            });
        }

        return Ok(new { Success = true, Data = result, Total = total, PageNumber = pageNumber, PageSize = pageSize, Errors = Array.Empty<string>() });
    }

    /// <summary>إنشاء مستخدم جديد</summary>
    [HttpPost("/api/users")]
    [HasPermission(Permissions.UsersEdit)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        if (await _userManager.FindByEmailAsync(command.Email) != null)
            return Conflict(new { Success = false, Message = $"Email '{command.Email}' is already registered.", Errors = new[] { "Duplicate email." } });

        var user = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email,
            FullName = command.FullName,
            BranchId = command.BranchId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
            return BadRequest(new { Success = false, Message = "Failed to create user.", Errors = result.Errors.Select(e => e.Description) });

        // Assign role if specified
        if (!string.IsNullOrWhiteSpace(command.RoleName) && await _roleManager.RoleExistsAsync(command.RoleName))
        {
            await _userManager.AddToRoleAsync(user, command.RoleName);
        }

        return Ok(new
        {
            Success = true,
            Message = $"User '{user.FullName}' created successfully.",
            Data = new { user.Id, user.FullName, user.Email },
            Errors = Array.Empty<string>()
        });
    }


    /// <summary>تعيين دور لمستخدم</summary>
    [HttpPost("/api/users/{userId}/assign-role")]
    [HasPermission(Permissions.UsersEdit)]
    public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleToUserCommand command)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Success = false, Message = "User not found." });

        var role = await _roleManager.FindByIdAsync(command.RoleId);
        if (role == null)
            return NotFound(new { Success = false, Message = "Role not found." });

        if (await _userManager.IsInRoleAsync(user, role.Name!))
            return Conflict(new { Success = false, Message = $"User already has role '{role.Name}'." });

        await _userManager.AddToRoleAsync(user, role.Name!);

        return Ok(new { Success = true, Message = $"Role '{role.Name}' assigned to user '{user.FullName}'.", Errors = Array.Empty<string>() });
    }

    /// <summary>إلغاء دور من مستخدم</summary>
    [HttpPost("/api/users/{userId}/remove-role")]
    [HasPermission(Permissions.UsersEdit)]
    public async Task<IActionResult> RemoveRole(string userId, [FromBody] AssignRoleToUserCommand command)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Success = false, Message = "User not found." });

        var role = await _roleManager.FindByIdAsync(command.RoleId);
        if (role == null)
            return NotFound(new { Success = false, Message = "Role not found." });

        await _userManager.RemoveFromRoleAsync(user, role.Name!);

        return Ok(new { Success = true, Message = $"Role '{role.Name}' removed from user '{user.FullName}'.", Errors = Array.Empty<string>() });
    }

    /// <summary>تفعيل / تعطيل مستخدم</summary>
    [HttpPost("/api/users/{userId}/toggle-active")]
    [HasPermission(Permissions.UsersEdit)]
    public async Task<IActionResult> ToggleUserActive(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Success = false, Message = "User not found." });

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        return Ok(new { Success = true, Message = $"User '{user.FullName}' is now {(user.IsActive ? "Active" : "Inactive")}.", Data = new { user.IsActive }, Errors = Array.Empty<string>() });
    }
}
