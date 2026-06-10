using FluentValidation;

namespace EnterpriseERP.Application.Features.Inventory.Queries.GetStockBalance;

public class GetStockBalanceQueryValidator : AbstractValidator<GetStockBalanceQuery>
{
    public GetStockBalanceQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than zero.");
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(1000).WithMessage("Page size must be between 1 and 1000.");
    }
}
