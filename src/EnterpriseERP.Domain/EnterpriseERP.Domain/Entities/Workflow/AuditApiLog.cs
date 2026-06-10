using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Workflow;

public class AuditApiLog : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ApiKeyId { get; set; }
    public Guid RequestId { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string? QueryParamsHash { get; set; }
    public string? RequestBodyHash { get; set; }
    public short ResponseStatus { get; set; }
    public int LatencyMs { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string ApiVersion { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
