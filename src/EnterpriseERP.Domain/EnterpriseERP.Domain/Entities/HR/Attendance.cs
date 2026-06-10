using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.HR;

public enum AttendanceStatus
{
    Present = 0,
    Late = 1,
    Absent = 2,
    Excused = 3
}

public class Attendance : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? CheckIn { get; set; }
    public TimeSpan? CheckOut { get; set; }
    public AttendanceStatus Status { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
