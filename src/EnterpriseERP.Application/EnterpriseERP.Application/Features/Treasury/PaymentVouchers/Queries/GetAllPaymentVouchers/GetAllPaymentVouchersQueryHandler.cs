using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Treasury.DTOs;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.PaymentVouchers.Queries.GetAllPaymentVouchers;

public class GetAllPaymentVouchersQueryHandler : IRequestHandler<GetAllPaymentVouchersQuery, PagedResult<PaymentVoucherDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllPaymentVouchersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<PaymentVoucherDto>> Handle(GetAllPaymentVouchersQuery request, CancellationToken cancellationToken)
    {
        var search = request.SearchTerm?.Trim();
        var (items, totalCount) = await _unitOfWork.Repository<PaymentVoucher>().GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            string.IsNullOrWhiteSpace(search)
                ? null
                : v => v.VoucherNumber.Contains(search) || v.ReferenceNumber.Contains(search));

        var voucherList = items.ToList();
        var supplierIds = voucherList.Where(v => v.SupplierId.HasValue).Select(v => v.SupplierId!.Value).Distinct().ToList();
        var cashIds = voucherList.Where(v => v.CashAccountId.HasValue).Select(v => v.CashAccountId!.Value).Distinct().ToList();
        var bankIds = voucherList.Where(v => v.BankAccountId.HasValue).Select(v => v.BankAccountId!.Value).Distinct().ToList();

        var suppliers = supplierIds.Count > 0
            ? (await _unitOfWork.Repository<Supplier>().FindAsync(s => supplierIds.Contains(s.Id))).ToDictionary(s => s.Id, s => s.Name)
            : new Dictionary<Guid, string>();

        var cashAccounts = cashIds.Count > 0
            ? (await _unitOfWork.Repository<CashAccount>().FindAsync(c => cashIds.Contains(c.Id))).ToDictionary(c => c.Id, c => c.Name)
            : new Dictionary<Guid, string>();

        var bankAccounts = bankIds.Count > 0
            ? (await _unitOfWork.Repository<BankAccount>().FindAsync(b => bankIds.Contains(b.Id))).ToDictionary(b => b.Id, b => b.BankName)
            : new Dictionary<Guid, string>();

        var dtos = voucherList.Select(v => new PaymentVoucherDto
        {
            Id = v.Id,
            VoucherNumber = v.VoucherNumber,
            VoucherDate = v.VoucherDate,
            SupplierId = v.SupplierId,
            PurchaseInvoiceId = v.PurchaseInvoiceId,
            SupplierName = v.SupplierId.HasValue && suppliers.TryGetValue(v.SupplierId.Value, out var sn) ? sn : string.Empty,
            CashAccountId = v.CashAccountId,
            CashAccountName = v.CashAccountId.HasValue && cashAccounts.TryGetValue(v.CashAccountId.Value, out var cashName) ? cashName : string.Empty,
            BankAccountId = v.BankAccountId,
            BankAccountName = v.BankAccountId.HasValue && bankAccounts.TryGetValue(v.BankAccountId.Value, out var bankName) ? bankName : string.Empty,
            Amount = v.Amount,
            ReferenceNumber = v.ReferenceNumber,
            Notes = v.Notes,
            Status = v.Status,
            StatusName = v.Status.ToString()
        });

        return new PagedResult<PaymentVoucherDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
