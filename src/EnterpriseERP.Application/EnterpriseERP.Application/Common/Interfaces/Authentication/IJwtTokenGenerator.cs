using EnterpriseERP.Domain.Entities.Identity;

namespace EnterpriseERP.Application.Common.Interfaces.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IList<string> roles, IList<string> permissions);
}
