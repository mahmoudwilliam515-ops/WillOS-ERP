using FluentValidation;

namespace EnterpriseERP.Application.Features.FixedAssets.Commands.CreateFixedAsset;

public class CreateFixedAssetCommandValidator : AbstractValidator<CreateFixedAssetCommand>
{
    public CreateFixedAssetCommandValidator()
    {
        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(v => v.NameAr)
            .NotEmpty().WithMessage("Arabic Name is required.")
            .MaximumLength(200).WithMessage("Arabic Name must not exceed 200 characters.");

        RuleFor(v => v.NameEn)
            .NotEmpty().WithMessage("English Name is required.")
            .MaximumLength(200).WithMessage("English Name must not exceed 200 characters.");

        RuleFor(v => v.PurchaseDate)
            .NotEmpty().WithMessage("Purchase Date is required.");

        RuleFor(v => v.PurchaseCost)
            .GreaterThan(0).WithMessage("Purchase Cost must be greater than zero.");

        RuleFor(v => v.SalvageValue)
            .GreaterThanOrEqualTo(0).WithMessage("Salvage Value cannot be negative.")
            .LessThan(v => v.PurchaseCost).WithMessage("Salvage Value must be less than Purchase Cost.");

        RuleFor(v => v.UsefulLifeYears)
            .GreaterThan(0).WithMessage("Useful Life Years must be greater than zero.");

        RuleFor(v => v.DepreciationMethod)
            .IsInEnum().WithMessage("Invalid Depreciation Method.");
            
        RuleFor(v => v.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.");
    }
}
