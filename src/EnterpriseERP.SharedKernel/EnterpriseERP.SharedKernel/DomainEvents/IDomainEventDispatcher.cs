using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.SharedKernel.DomainEvents;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearEvents(IEnumerable<BaseEntity> entitiesWithEvents);
}
