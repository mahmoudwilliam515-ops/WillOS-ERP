using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Settings;

public enum NotificationSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Success = 3
}

public class Notification : AuditableEntity, ITenantEntity
{
    // public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;
    public bool IsRead { get; set; }
    public string? UserId { get; set; } // Null for system-wide notifications
    public string? Link { get; set; } // Optional link to a page
}
