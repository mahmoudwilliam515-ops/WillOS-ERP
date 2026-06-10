using EnterpriseERP.Application.Features.SalesReturns.DTOs;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Application.Features.SalesReturns.Commands.CreateSalesReturn;

public class CreateSalesReturnCommand : IRequest<Guid>
{
    public Guid CompanyId { get; set; }
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
    public Guid OriginalInvoiceId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid WarehouseId { get; set; }
    public string ReturnReason { get; set; } = string.Empty;
    public decimal TaxPercent { get; set; } = 14; // Default VAT
    public List<SalesReturnLineDto> Lines { get; set; } = new();
}

public record SalesReturnLineDto
{
    public Guid OriginalInvoiceLineId { get; init; }
    public Guid ItemId { get; init; }
    public decimal ReturnQuantity { get; init; }
    public decimal UnitPrice { get; init; }
}
