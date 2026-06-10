using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Treasury.DTOs;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.ReceiptVouchers.Queries.GetAllReceiptVouchers;

public class GetAllReceiptVouchersQueryHandler : IRequestHandler<GetAllReceiptVouchersQuery, PagedResult<ReceiptVoucherDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllReceiptVouchersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ReceiptVoucherDto>> Handle(GetAllReceiptVouchersQuery request, CancellationToken cancellationToken)
    {
        var search = request.SearchTerm?.Trim();
        var (items, totalCount) = await _unitOfWork.Repository<ReceiptVoucher>().GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            string.IsNullOrWhiteSpace(search)
                ? null
                : v => v.VoucherNumber.Contains(search) || v.ReferenceNumber.Contains(search));

        var voucherList = items.ToList();
        var customerIds = voucherList.Where(v => v.CustomerId.HasValue).Select(v => v.CustomerId!.Value).Distinct().ToList();
        var cashIds = voucherList.Where(v => v.CashAccountId.HasValue).Select(v => v.CashAccountId!.Value).Distinct().ToList();
        var bankIds = voucherList.Where(v => v.BankAccountId.HasValue).Select(v => v.BankAccountId!.Value).Distinct().ToList();

        var customers = customerIds.Count > 0
            ? (await _unitOfWork.Repository<Customer>().FindAsync(c => customerIds.Contains(c.Id))).ToDictionary(c => c.Id, c => c.Name)
            : new Dictionary<Guid, string>();

        var cashAccounts = cashIds.Count > 0
            ? (await _unitOfWork.Repository<CashAccount>().FindAsync(c => cashIds.Contains(c.Id))).ToDictionary(c => c.Id, c => c.Name)
            : new Dictionary<Guid, string>();

        var bankAccounts = bankIds.Count > 0
            ? (await _unitOfWork.Repository<BankAccount>().FindAsync(b => bankIds.Contains(b.Id))).ToDictionary(b => b.Id, b => b.BankName)
            : new Dictionary<Guid, string>();

        var dtos = voucherList.Select(v => new ReceiptVoucherDto
        {
            Id = v.Id,
            VoucherNumber = v.VoucherNumber,
            VoucherDate = v.VoucherDate,
            CustomerId = v.CustomerId,
            SalesInvoiceId = v.SalesInvoiceId,
            CustomerName = v.CustomerId.HasValue && customers.TryGetValue(v.CustomerId.Value, out var cn) ? cn : string.Empty,
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

        return new PagedResult<ReceiptVoucherDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
