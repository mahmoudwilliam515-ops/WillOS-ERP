using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Infrastructure.Identity;

/// <summary>
/// Granular permission assigned to a role.
/// Permission-Based Authorization: every action requires explicit permission.
/// </summary>
public class RolePermission : BaseEntity
{
    public string RoleId { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;   // e.g., "Invoices.Create"
    public string Module { get; set; } = string.Empty;       // e.g., "Sales"

    public ApplicationRole Role { get; set; } = null!;
}
