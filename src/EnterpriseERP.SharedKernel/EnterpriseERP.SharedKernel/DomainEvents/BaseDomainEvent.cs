using MediatR;

namespace EnterpriseERP.SharedKernel.DomainEvents;

public abstract class BaseDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
}
