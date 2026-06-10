namespace EnterpriseERP.SharedKernel.Common;

public abstract class AggregateRoot : BaseEntity, IAggregateRoot
{
    // By inheriting from BaseEntity, it gets Id and Domain Events functionality.
    // In strict DDD, only Aggregate Roots should have Domain Events.
    // However, since BaseEntity already has them for backward compatibility,
    // this class serves as an explicit structural marker for new domains.
}
