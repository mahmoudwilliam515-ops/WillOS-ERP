using FluentValidation;

namespace EnterpriseERP.Application.Features.SalesQuotations.Commands.CreateSalesQuotation;

public class CreateSalesQuotationCommandValidator : AbstractValidator<CreateSalesQuotationCommand>
{
    public CreateSalesQuotationCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Customer is required.");
        RuleFor(x => x.BranchId).NotEmpty().WithMessage("Branch is required.");
        RuleFor(x => x.ValidUntil).GreaterThanOrEqualTo(x => x.QuotationDate).WithMessage("Valid until date must be after or equal to quotation date.");
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0).WithMessage("Total amount cannot be negative.");
    }
}
