using EnterpriseERP.SharedKernel.Common;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseERP.Domain.Entities.Audit;

public class AuditLog : AuditableEntity
{
    public string EventType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    
    // Append-Only: Hash for integrity verification (SHA-256)
    [MaxLength(64)]
    public string Hash { get; set; } = string.Empty;
}
