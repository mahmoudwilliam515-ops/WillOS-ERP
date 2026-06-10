using EnterpriseERP.SharedKernel.Exceptions;

namespace EnterpriseERP.Domain.Exceptions;

public class SalesDomainException : DomainException
{
    public SalesDomainException(string message) : base(message) { }
    public SalesDomainException(string message, Exception innerException) : base(message, innerException) { }
}
