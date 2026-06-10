// This behavior now delegates to the SharedKernel centralized ValidationBehavior.
// Kept for backward compatibility with DependencyInjection registration.
using FluentValidation;
using MediatR;
using EnterpriseERP.SharedKernel.Behaviors;

namespace EnterpriseERP.Application.Common.Behaviors;

// Alias: re-export so existing DI registration continues to work unchanged.
public class ValidationBehavior<TRequest, TResponse>
    : EnterpriseERP.SharedKernel.Behaviors.ValidationBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        : base(validators) { }
}
