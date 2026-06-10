using EnterpriseERP.SharedKernel.Common;
using System;
using System.Collections.Generic;
using EnterpriseERP.Domain.Common;

namespace EnterpriseERP.Domain.Entities.Reporting;

public class ReportTemplate : AuditableEntity, IAggregateRoot, ICompanyEntity
{
    public Guid CompanyId { get; set; }
    public EnterpriseERP.Domain.Entities.Settings.Company Company { get; set; } = null!;

    public string ReportType { get; set; } = string.Empty; // 'PL', 'BS', 'CF'
    public string GaapBook { get; set; } = "IFRS";
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<ReportSection> Sections { get; set; } = new List<ReportSection>();
}
