using EnterpriseERP.SharedKernel.Common;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Reporting;

public class ReportLine : AuditableEntity
{
    public Guid SectionId { get; set; }
    public ReportSection Section { get; set; } = null!;

    public string LineCode { get; set; } = string.Empty;
    public string LineName { get; set; } = string.Empty;
    public string? LineNameAr { get; set; }
    public int SortOrder { get; set; }

    public string? AccountFrom { get; set; }
    public string? AccountTo { get; set; }

    // Use a string or JSON for AccountIds if PostgreSQL arrays aren't supported natively without Npgsql configurations, 
    // but in EF Core we can use simple List of strings if converted properly.
    public string? AccountIdsCsv { get; set; } 

    public string Aggregation { get; set; } = "SUM"; // e.g. SUM
}
