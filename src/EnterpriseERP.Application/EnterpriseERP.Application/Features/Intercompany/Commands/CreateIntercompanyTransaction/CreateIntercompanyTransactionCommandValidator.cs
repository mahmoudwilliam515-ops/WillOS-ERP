using FluentValidation;

namespace EnterpriseERP.Application.Features.Intercompany.Commands.CreateIntercompanyTransaction;

public class CreateIntercompanyTransactionCommandValidator : AbstractValidator<CreateIntercompanyTransactionCommand>
{
    public CreateIntercompanyTransactionCommandValidator()
    {
        RuleFor(x => x.SourceCompanyId).NotEmpty();
        RuleFor(x => x.TargetCompanyId).NotEmpty();
        RuleFor(x => x).Must(x => x.SourceCompanyId != x.TargetCompanyId)
            .WithMessage("Source and target companies must be different.");
        RuleFor(x => x.TransactionDate).NotEmpty();
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
