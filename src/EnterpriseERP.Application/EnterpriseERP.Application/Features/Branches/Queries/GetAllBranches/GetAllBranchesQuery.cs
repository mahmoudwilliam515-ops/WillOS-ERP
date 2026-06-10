using EnterpriseERP.Application.Features.Branches.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Branches.Queries.GetAllBranches;

public class GetAllBranchesQuery : IRequest<IEnumerable<BranchDto>>
{
}
