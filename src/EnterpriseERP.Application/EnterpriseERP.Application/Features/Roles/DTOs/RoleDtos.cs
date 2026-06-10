namespace EnterpriseERP.Application.Features.Roles.DTOs;

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class CreateRoleCommand
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class UpdateRolePermissionsCommand
{
    public string RoleId { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public class AssignRoleToUserCommand
{
    public string UserId { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid? BranchId { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime? LastLoginAt { get; set; }
}

public class CreateUserCommand
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string? RoleName { get; set; }
}
