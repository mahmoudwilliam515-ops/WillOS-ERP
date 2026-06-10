using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Manufacturing;

public class WorkCenterCalendar : AuditableEntity, IAggregateRoot
{
    public Guid WorkCenterId { get; set; }
    public WorkCenter WorkCenter { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsWorkingDay { get; set; } = true;
    
    // Total available hours for this day (can be used for capacity calculation)
    public decimal AvailableHours => IsWorkingDay ? (decimal)(EndTime - StartTime).TotalHours : 0;
}

public class WorkCenterException : AuditableEntity
{
    public Guid WorkCenterId { get; set; }
    public WorkCenter WorkCenter { get; set; } = null!;

    public DateTime Date { get; set; }
    public bool IsWorkingDay { get; set; } // Can be false for holidays or true for extra shifts
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
}
