using EnterpriseERP.SharedKernel.Exceptions;

namespace EnterpriseERP.Domain.Exceptions;

public class PurchasingDomainException : DomainException
{
    public PurchasingDomainException(string message) : base(message) { }
    public PurchasingDomainException(string message, Exception innerException) : base(message, innerException) { }
}
