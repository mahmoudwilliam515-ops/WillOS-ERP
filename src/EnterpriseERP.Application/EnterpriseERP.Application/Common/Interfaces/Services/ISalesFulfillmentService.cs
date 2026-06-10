using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Sales;
using System;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface ISalesFulfillmentService
{
    Task<Guid> CreateDeliveryFromOrderAsync(Guid salesOrderId, DateTime deliveryDate);
    Task<Guid> CreateInvoiceFromDeliveryAsync(Guid deliveryNoteId, DateTime invoiceDate);
}
