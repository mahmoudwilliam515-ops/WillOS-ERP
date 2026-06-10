using FluentValidation;

namespace EnterpriseERP.Application.Features.Companies.Commands.CreateCompany;

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CountryCode).NotEmpty().Length(2, 3);
        RuleFor(x => x.FunctionalCurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.TaxRegistrationNumber).MaximumLength(100);
    }
}
