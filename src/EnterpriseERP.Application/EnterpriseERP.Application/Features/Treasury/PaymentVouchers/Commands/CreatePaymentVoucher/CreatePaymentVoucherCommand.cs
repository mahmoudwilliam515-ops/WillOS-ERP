using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.PaymentVouchers.Commands.CreatePaymentVoucher;

public class CreatePaymentVoucherCommand : IRequest<Guid>
{
    public DateTime VoucherDate { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? CashAccountId { get; set; }
    public Guid? BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<PaymentVoucherAllocationDto> Allocations { get; set; } = new();
}

public class PaymentVoucherAllocationDto
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
}
