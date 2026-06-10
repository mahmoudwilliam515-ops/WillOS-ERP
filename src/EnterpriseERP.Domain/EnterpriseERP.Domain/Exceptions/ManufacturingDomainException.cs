using EnterpriseERP.SharedKernel.Exceptions;

namespace EnterpriseERP.Domain.Exceptions;

public class ManufacturingDomainException : DomainException
{
    public ManufacturingDomainException(string message) : base(message) { }
    public ManufacturingDomainException(string message, Exception innerException) : base(message, innerException) { }
}
