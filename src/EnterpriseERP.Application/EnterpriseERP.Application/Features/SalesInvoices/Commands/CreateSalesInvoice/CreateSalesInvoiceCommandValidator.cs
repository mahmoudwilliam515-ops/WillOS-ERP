using EnterpriseERP.Application.Features.SalesInvoices.DTOs;
using FluentValidation;

namespace EnterpriseERP.Application.Features.SalesInvoices.Commands.CreateSalesInvoice;

public class CreateSalesInvoiceCommandValidator : AbstractValidator<CreateSalesInvoiceCommand>
{
    public CreateSalesInvoiceCommandValidator()
    {
        RuleFor(v => v.CustomerId)
            .NotEmpty().WithMessage("Customer is required.");

        RuleFor(v => v.BranchId)
            .NotEmpty().WithMessage("Branch is required.");

        RuleFor(v => v.WarehouseId)
            .NotEmpty().WithMessage("Warehouse is required.");

        RuleFor(v => v.InvoiceDate)
            .NotEmpty().WithMessage("Invoice date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Invoice date cannot be in the future.");

        RuleFor(v => v.Lines)
            .NotEmpty().WithMessage("Invoice must have at least one line item.");

        RuleForEach(v => v.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemId)
                .NotEmpty().WithMessage("Item is required for each line.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            line.RuleFor(l => l.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than zero.");

            line.RuleFor(l => l.DiscountPercent)
                .InclusiveBetween(0, 100).WithMessage("Discount percent must be between 0 and 100.");
        });

        RuleFor(v => v.PaidAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Paid amount must be zero or greater.");
    }
}
