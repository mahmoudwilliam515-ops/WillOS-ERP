using EnterpriseERP.SharedKernel.Common;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Reporting;

public class ReportSection : AuditableEntity
{
    public Guid TemplateId { get; set; }
    public ReportTemplate Template { get; set; } = null!;

    public string SectionCode { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string? SectionNameAr { get; set; }

    public Guid? ParentId { get; set; }
    public ReportSection? Parent { get; set; }

    public int SortOrder { get; set; }
    public string NormalBalance { get; set; } = "D"; // 'D' or 'C'
    public bool Subtotal { get; set; } = false;
    public bool Negate { get; set; } = false;

    public ICollection<ReportSection> SubSections { get; set; } = new List<ReportSection>();
    public ICollection<ReportLine> Lines { get; set; } = new List<ReportLine>();
}
