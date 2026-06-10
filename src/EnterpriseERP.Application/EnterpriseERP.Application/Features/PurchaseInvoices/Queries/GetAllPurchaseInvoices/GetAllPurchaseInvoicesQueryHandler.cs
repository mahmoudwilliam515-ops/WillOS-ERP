using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.SharedKernel.Results;
using EnterpriseERP.Application.Features.PurchaseInvoices.DTOs;
using EnterpriseERP.Domain.Entities.Purchasing;
using MediatR;

namespace EnterpriseERP.Application.Features.PurchaseInvoices.Queries.GetAllPurchaseInvoices;

public class GetAllPurchaseInvoicesQueryHandler : IRequestHandler<GetAllPurchaseInvoicesQuery, PagedResult<PurchaseInvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllPurchaseInvoicesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<PurchaseInvoiceDto>> Handle(GetAllPurchaseInvoicesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Repository<PurchaseInvoice>()
            .GetPagedAsync(request.PageNumber, request.PageSize, string.IsNullOrWhiteSpace(request.SearchTerm) ? null : i => i.InvoiceNumber.Contains(request.SearchTerm));

        var dtos = items.Select(inv => new PurchaseInvoiceDto
        {
            Id = inv.Id,
            InvoiceNumber = inv.InvoiceNumber,
            SupplierInvoiceNumber = inv.SupplierInvoiceNumber,
            InvoiceDate = inv.InvoiceDate,
            SupplierId = inv.SupplierId,
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
            Lines = new List<PurchaseInvoiceLineDto>()
        });

        return new PagedResult<PurchaseInvoiceDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
