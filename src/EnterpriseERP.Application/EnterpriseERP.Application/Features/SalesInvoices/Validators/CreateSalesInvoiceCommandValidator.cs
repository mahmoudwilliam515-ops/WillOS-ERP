using FluentValidation;
using EnterpriseERP.Application.Features.SalesInvoices.DTOs;

namespace EnterpriseERP.Application.Features.SalesInvoices.Validators;

public class CreateSalesInvoiceCommandValidator : AbstractValidator<CreateSalesInvoiceCommand>
{
    public CreateSalesInvoiceCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer is required.");

        RuleFor(x => x.BranchId)
            .NotEmpty()
            .WithMessage("Branch is required.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .WithMessage("Warehouse is required.");

        RuleFor(x => x.InvoiceDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Invoice date cannot be in the future.");

        RuleFor(x => x.PaidAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Paid amount cannot be negative.");

        RuleFor(x => x.Lines)
            .NotEmpty()
            .WithMessage("Invoice must have at least one line item.");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemId)
                .NotEmpty()
                .WithMessage("Each line must have a valid Item.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");

            line.RuleFor(l => l.UnitPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Unit price cannot be negative.");

            line.RuleFor(l => l.DiscountPercent)
                .InclusiveBetween(0, 100)
                .WithMessage("Discount must be between 0% and 100%.");

            line.RuleFor(l => l.TaxPercent)
                .InclusiveBetween(0, 100)
                .WithMessage("Tax must be between 0% and 100%.");
        });
    }
}
