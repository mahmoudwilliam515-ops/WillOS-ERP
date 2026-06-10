using EnterpriseERP.SharedKernel.Exceptions;

namespace EnterpriseERP.Domain.Exceptions;

public class HRDomainException : DomainException
{
    public HRDomainException(string message) : base(message) { }
    public HRDomainException(string message, Exception innerException) : base(message, innerException) { }
}
