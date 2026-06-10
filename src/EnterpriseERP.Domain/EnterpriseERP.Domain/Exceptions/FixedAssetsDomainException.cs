using EnterpriseERP.SharedKernel.Exceptions;

namespace EnterpriseERP.Domain.Exceptions;

public class FixedAssetsDomainException : DomainException
{
    public FixedAssetsDomainException(string message) : base(message) { }
    public FixedAssetsDomainException(string message, Exception innerException) : base(message, innerException) { }
}
