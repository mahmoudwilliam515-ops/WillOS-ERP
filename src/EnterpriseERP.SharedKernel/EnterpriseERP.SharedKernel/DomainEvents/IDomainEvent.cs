using MediatR;

namespace EnterpriseERP.SharedKernel.DomainEvents;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
