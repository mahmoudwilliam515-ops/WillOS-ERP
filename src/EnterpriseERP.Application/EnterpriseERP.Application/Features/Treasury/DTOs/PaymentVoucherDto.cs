using EnterpriseERP.Domain.Entities.Treasury;

namespace EnterpriseERP.Application.Features.Treasury.DTOs;

public class PaymentVoucherDto
{
    public Guid Id { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? PurchaseInvoiceId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
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
