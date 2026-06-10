using Microsoft.AspNetCore.Authorization;

namespace EnterpriseERP.Infrastructure.Identity.Authorization;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
        : base(policy: $"Permission:{permission}")
    {
    }
}
