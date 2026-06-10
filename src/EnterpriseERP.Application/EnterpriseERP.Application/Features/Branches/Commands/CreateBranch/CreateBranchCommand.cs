using EnterpriseERP.Application.Features.Branches.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Branches.Commands.CreateBranch;

public class CreateBranchCommand : IRequest<BranchDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
}
