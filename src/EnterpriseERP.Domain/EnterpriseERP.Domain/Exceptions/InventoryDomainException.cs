using EnterpriseERP.SharedKernel.Exceptions;

namespace EnterpriseERP.Domain.Exceptions;

public class InventoryDomainException : DomainException
{
    public InventoryDomainException(string message) : base(message) { }
    public InventoryDomainException(string message, Exception innerException) : base(message, innerException) { }
}
