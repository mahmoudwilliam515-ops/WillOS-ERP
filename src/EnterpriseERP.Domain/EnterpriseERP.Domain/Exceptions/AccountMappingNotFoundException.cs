using EnterpriseERP.SharedKernel.Exceptions;

namespace EnterpriseERP.Domain.Exceptions;

public class AccountMappingNotFoundException : DomainException
{
    public AccountMappingNotFoundException(string message)
        : base(message) { }
}
