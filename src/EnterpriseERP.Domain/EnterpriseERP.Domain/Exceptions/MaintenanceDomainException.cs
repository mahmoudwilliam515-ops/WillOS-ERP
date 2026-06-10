using EnterpriseERP.SharedKernel.Exceptions;

namespace EnterpriseERP.Domain.Exceptions;

public class MaintenanceDomainException : DomainException
{
    public MaintenanceDomainException(string message) : base(message) { }
    public MaintenanceDomainException(string message, Exception innerException) : base(message, innerException) { }
}
