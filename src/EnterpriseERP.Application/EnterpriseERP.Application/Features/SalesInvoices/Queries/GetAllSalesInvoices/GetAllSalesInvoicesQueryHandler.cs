using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Application.Features.SalesInvoices.DTOs;
using EnterpriseERP.Domain.Entities.Sales;
using MediatR;

namespace EnterpriseERP.Application.Features.SalesInvoices.Queries.GetAllSalesInvoices;

public class GetAllSalesInvoicesQueryHandler : IRequestHandler<GetAllSalesInvoicesQuery, PagedResult<SalesInvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSalesInvoicesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<SalesInvoiceDto>> Handle(GetAllSalesInvoicesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Repository<SalesInvoice>()
            .GetPagedAsync(request.PageNumber, request.PageSize, string.IsNullOrWhiteSpace(request.SearchTerm) ? null : i => i.InvoiceNumber.Contains(request.SearchTerm));

        var dtos = items.Select(inv => new SalesInvoiceDto
        {
            Id = inv.Id,
            InvoiceNumber = inv.InvoiceNumber,
            InvoiceDate = inv.InvoiceDate,
            CustomerId = inv.CustomerId,
            BranchId = inv.BranchId,
            WarehouseId = inv.WarehouseId,
            SubTotal = inv.SubTotal,
            DiscountAmount = inv.DiscountAmount,
            TaxAmount = inv.TaxAmount,
            TotalAmount = inv.TotalAmount,
            PaidAmount = inv.PaidAmount,
            RemainingAmount = inv.RemainingAmount,
            Status = inv.Status.ToString(),
            Notes = inv.Notes,
            Lines = new List<SalesInvoiceLineDto>()
        });

        return new PagedResult<SalesInvoiceDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
