using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Application.Features.SalesQuotations.Queries.GetAllSalesQuotations;

public class GetAllSalesQuotationsQuery : IRequest<PagedResult<SalesQuotationDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
}

public record SalesQuotationDto
{
    public Guid Id { get; init; }
    public string QuotationNumber { get; init; } = string.Empty;
    public DateTime QuotationDate { get; init; }
    public DateTime ValidUntil { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
}
