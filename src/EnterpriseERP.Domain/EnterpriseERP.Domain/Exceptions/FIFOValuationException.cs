using EnterpriseERP.SharedKernel.Exceptions;

namespace EnterpriseERP.Domain.Exceptions;

public class FIFOValuationException : DomainException
{
    public FIFOValuationException(string message)
        : base(message) { }
}
