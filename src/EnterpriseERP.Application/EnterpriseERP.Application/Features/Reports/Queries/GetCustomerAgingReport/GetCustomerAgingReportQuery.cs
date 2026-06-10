using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Reports.DTOs;
using EnterpriseERP.Domain.Entities.Sales;
using MediatR;

namespace EnterpriseERP.Application.Features.Reports.Queries.GetCustomerAgingReport;

public record GetCustomerAgingReportQuery : IRequest<List<CustomerAgingReportDto>>
{
}

public class GetCustomerAgingReportQueryHandler : IRequestHandler<GetCustomerAgingReportQuery, List<CustomerAgingReportDto>>
{
    private readonly IGenericRepository<SalesInvoice> _salesInvoiceRepo;
    private readonly IGenericRepository<Customer> _customerRepo;

    public GetCustomerAgingReportQueryHandler(
        IGenericRepository<SalesInvoice> salesInvoiceRepo,
        IGenericRepository<Customer> customerRepo)
    {
        _salesInvoiceRepo = salesInvoiceRepo;
        _customerRepo = customerRepo;
    }

    public async Task<List<CustomerAgingReportDto>> Handle(GetCustomerAgingReportQuery request, CancellationToken cancellationToken)
    {
        // Get all outstanding approved invoices
        var invoicesResult = await _salesInvoiceRepo.FindAsync(i => i.Status == InvoiceStatus.Approved && i.RemainingAmount > 0);
        var invoices = invoicesResult.ToList();

        // Group by CustomerId
        var groupedInvoices = invoices.GroupBy(i => i.CustomerId).ToList();
        var customerIds = groupedInvoices.Select(g => g.Key).ToList();

        // Get customers info
        var customersResult = await _customerRepo.FindAsync(c => customerIds.Contains(c.Id));
        var customers = customersResult.ToDictionary(c => c.Id, c => c.Name);

        var report = new List<CustomerAgingReportDto>();
        var currentDate = DateTime.UtcNow;

        foreach (var group in groupedInvoices)
        {
            var customerId = group.Key;
            var customerName = customers.GetValueOrDefault(customerId, "Unknown");

            var dto = new CustomerAgingReportDto
            {
                CustomerId = customerId,
                CustomerName = customerName,
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
