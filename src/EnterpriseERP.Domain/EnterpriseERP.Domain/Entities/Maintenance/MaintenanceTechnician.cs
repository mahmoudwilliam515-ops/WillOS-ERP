using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Entities.HR;
using System;

namespace EnterpriseERP.Domain.Entities.Maintenance;

public class MaintenanceTechnician : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    
    public string Specialization { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public bool IsActive { get; set; } = true;
}
