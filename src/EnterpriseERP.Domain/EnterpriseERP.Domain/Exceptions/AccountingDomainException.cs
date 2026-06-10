using EnterpriseERP.SharedKernel.Exceptions;

namespace EnterpriseERP.Domain.Exceptions;

public class AccountingDomainException : DomainException
{
    public AccountingDomainException(string message) : base(message) { }
    public AccountingDomainException(string message, Exception innerException) : base(message, innerException) { }
}
