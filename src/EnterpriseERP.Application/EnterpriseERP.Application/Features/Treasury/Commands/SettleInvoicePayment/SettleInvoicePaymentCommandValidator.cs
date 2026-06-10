using FluentValidation;

namespace EnterpriseERP.Application.Features.Treasury.Commands.SettleInvoicePayment;

public class SettleInvoicePaymentCommandValidator
    : AbstractValidator<SettleInvoicePaymentCommand>
{
    public SettleInvoicePaymentCommandValidator()
    {
        RuleFor(x => x.ReceiptVoucherId)
            .NotEmpty().WithMessage("ReceiptVoucherId مطلوب");

        RuleFor(x => x.SalesInvoiceId)
            .NotEmpty().WithMessage("SalesInvoiceId مطلوب");

        RuleFor(x => x.AllocatedAmount)
            .GreaterThan(0).WithMessage("AllocatedAmount يجب أن يكون موجباً");

        RuleFor(x => x.SettlementDate)
            .NotEmpty().WithMessage("SettlementDate مطلوب")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("لا يمكن تسجيل دفعة بتاريخ مستقبلي");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId مطلوب");
    }
}
