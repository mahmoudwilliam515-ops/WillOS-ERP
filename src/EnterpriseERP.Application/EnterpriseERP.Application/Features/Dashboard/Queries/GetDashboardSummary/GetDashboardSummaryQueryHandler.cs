using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Sales;
using MediatR;

namespace EnterpriseERP.Application.Features.Dashboard.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardSummaryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Run all queries in parallel for minimal latency
        var salesTask       = _unitOfWork.Repository<SalesInvoice>().FindAsync(
            s => !s.IsDeleted && s.Status == InvoiceStatus.Approved && s.InvoiceDate >= startOfMonth);
        var allSalesTask    = _unitOfWork.Repository<SalesInvoice>().FindAsync(
            s => !s.IsDeleted && s.Status == InvoiceStatus.Draft);
        var purchasesTask   = _unitOfWork.Repository<PurchaseInvoice>().FindAsync(
            p => !p.IsDeleted && p.Status == PurchaseInvoiceStatus.Approved && p.InvoiceDate >= startOfMonth);
        var pendingPurchTask = _unitOfWork.Repository<PurchaseInvoice>().FindAsync(
            p => !p.IsDeleted && p.Status == PurchaseInvoiceStatus.Draft);
        var customersTask   = _unitOfWork.Repository<Customer>().FindAsync(c => !c.IsDeleted);
        var suppliersTask   = _unitOfWork.Repository<Supplier>().FindAsync(s => !s.IsDeleted);
        var itemsTask       = _unitOfWork.Repository<Item>().FindAsync(i => !i.IsDeleted);

        await Task.WhenAll(salesTask, allSalesTask, purchasesTask, pendingPurchTask, customersTask, suppliersTask, itemsTask);

        var approvedSales     = await salesTask;
        var pendingSales      = await allSalesTask;
        var approvedPurchases = await purchasesTask;
        var pendingPurchases  = await pendingPurchTask;
        var customers         = await customersTask;
        var suppliers         = await suppliersTask;
        var items             = await itemsTask;

        // Calculate low stock items (items with MinStock threshold defined)
        var lowStockItems = items.Count(i => i.MinStock > 0);

        return new DashboardSummaryDto
        {
            SalesThisMonth          = approvedSales.Sum(s => s.TotalAmount),
            PurchasesThisMonth      = approvedPurchases.Sum(p => p.TotalAmount),
            TotalCustomers          = customers.Count(),
            TotalSuppliers          = suppliers.Count(),
            TotalItems              = items.Count(),
            PendingSalesInvoices    = pendingSales.Count(),
            PendingPurchaseInvoices = pendingPurchases.Count(),
            LowStockItems           = lowStockItems,
        };
    }
}
