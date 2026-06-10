namespace EnterpriseERP.Application.Features.Dashboard.Queries.GetDashboardSummary;

public class DashboardSummaryDto
{
    public decimal SalesThisMonth { get; set; }
    public decimal PurchasesThisMonth { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalSuppliers { get; set; }
    public int TotalItems { get; set; }
    public int PendingSalesInvoices { get; set; }
    public int PendingPurchaseInvoices { get; set; }
    public int LowStockItems { get; set; }
}
