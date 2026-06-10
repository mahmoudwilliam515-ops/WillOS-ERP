using EnterpriseERP.Application.Features.Treasury.DTOs;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.PaymentVouchers.Queries.GetAllPaymentVouchers;

public class GetAllPaymentVouchersQuery : IRequest<PagedResult<PaymentVoucherDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SearchTerm { get; set; }
}
