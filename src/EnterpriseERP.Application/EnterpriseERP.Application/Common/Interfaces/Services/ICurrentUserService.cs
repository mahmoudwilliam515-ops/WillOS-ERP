namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    Guid? TenantId { get; }
    Guid? CompanyId { get; }
    string? FullName { get; }
    Guid? BranchId { get; }
    bool IsAuthenticated { get; }
}
