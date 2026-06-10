
namespace EnterpriseERP.SharedKernel.Common;

public abstract class AuditableEntity : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
