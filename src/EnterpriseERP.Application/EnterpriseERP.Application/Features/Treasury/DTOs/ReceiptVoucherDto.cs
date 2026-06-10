using EnterpriseERP.Domain.Entities.Treasury;

namespace EnterpriseERP.Application.Features.Treasury.DTOs;

public class ReceiptVoucherDto
{
    public Guid Id { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? SalesInvoiceId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? CashAccountId { get; set; }
    public string CashAccountName { get; set; } = string.Empty;
    public Guid? BankAccountId { get; set; }
    public string BankAccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public VoucherStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
}
