using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Logistics.DeliveryNotes.Commands.CreateDeliveryNote;

public class CreateDeliveryNoteCommand : IRequest<Result<Guid>>
{
    public DateTime DeliveryDate { get; set; } = DateTime.UtcNow;
    public Guid? SalesOrderId { get; set; }
    public Guid? SalesInvoiceId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid WarehouseId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
