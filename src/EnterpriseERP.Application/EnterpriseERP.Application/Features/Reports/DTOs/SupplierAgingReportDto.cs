namespace EnterpriseERP.Application.Features.Reports.DTOs;

public class SupplierAgingReportDto
{
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalDue { get; set; }
    public decimal Current { get; set; }        // 0-30 Days
    public decimal Days31To60 { get; set; }     // 31-60 Days
    public decimal Days61To90 { get; set; }     // 61-90 Days
    public decimal Days91To120 { get; set; }    // 91-120 Days
    public decimal Over120Days { get; set; }    // > 120 Days
}
