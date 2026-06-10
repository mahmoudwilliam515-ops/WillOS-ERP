using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.ReceiptVouchers.Commands.CreateReceiptVoucher;

public class CreateReceiptVoucherCommand : IRequest<Guid>
{
    public DateTime VoucherDate { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? CashAccountId { get; set; }
    public Guid? BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<VoucherAllocationDto> Allocations { get; set; } = new();
}

public class VoucherAllocationDto
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
}
