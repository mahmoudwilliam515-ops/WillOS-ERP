using EnterpriseERP.SharedKernel.Exceptions;

namespace EnterpriseERP.Domain.Exceptions;

public class ProjectDomainException : DomainException
{
    public ProjectDomainException(string message) : base(message) { }
    public ProjectDomainException(string message, Exception innerException) : base(message, innerException) { }
}
