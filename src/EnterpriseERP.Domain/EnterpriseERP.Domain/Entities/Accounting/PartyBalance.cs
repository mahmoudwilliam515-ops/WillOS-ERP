using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.Accounting;

public enum PartyType
{
    Customer = 1,
    Supplier = 2,
    Employee = 3
}

public class PartyBalance : AuditableEntity, IAggregateRoot
{
    public Guid PartyId { get; set; }
    public PartyType PartyType { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal NetBalance => TotalDebit - TotalCredit;
    public DateTime LastUpdated { get; set; }
}
