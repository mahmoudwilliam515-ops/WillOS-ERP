using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Reports.DTOs;
using EnterpriseERP.Domain.Entities.Purchasing;
using MediatR;

namespace EnterpriseERP.Application.Features.Reports.Queries.GetSupplierAgingReport;

public record GetSupplierAgingReportQuery : IRequest<List<SupplierAgingReportDto>>
{
}

public class GetSupplierAgingReportQueryHandler : IRequestHandler<GetSupplierAgingReportQuery, List<SupplierAgingReportDto>>
{
    private readonly IGenericRepository<PurchaseInvoice> _purchaseInvoiceRepo;
    private readonly IGenericRepository<Supplier> _supplierRepo;

    public GetSupplierAgingReportQueryHandler(
        IGenericRepository<PurchaseInvoice> purchaseInvoiceRepo,
        IGenericRepository<Supplier> supplierRepo)
    {
        _purchaseInvoiceRepo = purchaseInvoiceRepo;
        _supplierRepo = supplierRepo;
    }

    public async Task<List<SupplierAgingReportDto>> Handle(GetSupplierAgingReportQuery request, CancellationToken cancellationToken)
    {
        // Get all outstanding approved purchase invoices
        var invoicesResult = await _purchaseInvoiceRepo.FindAsync(
            i => i.Status == PurchaseInvoiceStatus.Approved && i.RemainingAmount > 0);
        var invoices = invoicesResult.ToList();

        var groupedInvoices = invoices.GroupBy(i => i.SupplierId).ToList();
        var supplierIds = groupedInvoices.Select(g => g.Key).ToList();

        var suppliersResult = await _supplierRepo.FindAsync(s => supplierIds.Contains(s.Id));
        var suppliers = suppliersResult.ToDictionary(s => s.Id, s => s.Name);

        var report = new List<SupplierAgingReportDto>();
        var currentDate = DateTime.UtcNow;

        foreach (var group in groupedInvoices)
        {
            var supplierId = group.Key;
            var supplierName = suppliers.GetValueOrDefault(supplierId, "Unknown");

            var dto = new SupplierAgingReportDto
            {
                SupplierId = supplierId,
                SupplierName = supplierName,
                TotalDue = 0,
                Current = 0,
                Days31To60 = 0,
                Days61To90 = 0,
                Days91To120 = 0,
                Over120Days = 0
            };

            foreach (var invoice in group)
            {
                dto.TotalDue += invoice.RemainingAmount;

                var ageInDays = (currentDate - invoice.InvoiceDate).TotalDays;

                if (ageInDays <= 30)
                    dto.Current += invoice.RemainingAmount;
                else if (ageInDays <= 60)
                    dto.Days31To60 += invoice.RemainingAmount;
                else if (ageInDays <= 90)
                    dto.Days61To90 += invoice.RemainingAmount;
                else if (ageInDays <= 120)
                    dto.Days91To120 += invoice.RemainingAmount;
                else
                    dto.Over120Days += invoice.RemainingAmount;
            }

            report.Add(dto);
        }

        return report.OrderByDescending(r => r.TotalDue).ToList();
    }
}
