namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IOrderNumberGenerator
{
    Task<string> GenerateAsync(string prefix, Guid companyId, Guid tenantId, CancellationToken cancellationToken = default);
}
