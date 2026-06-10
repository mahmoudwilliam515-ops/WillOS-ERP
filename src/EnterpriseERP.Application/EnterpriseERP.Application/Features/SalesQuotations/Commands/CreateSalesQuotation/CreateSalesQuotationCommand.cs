using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.SalesQuotations.Commands.CreateSalesQuotation;

public class CreateSalesQuotationCommand : IRequest<Result<Guid>>
{
    public DateTime QuotationDate { get; set; }
    public DateTime ValidUntil { get; set; }
    public Guid CustomerId { get; set; }
    public Guid BranchId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
}
