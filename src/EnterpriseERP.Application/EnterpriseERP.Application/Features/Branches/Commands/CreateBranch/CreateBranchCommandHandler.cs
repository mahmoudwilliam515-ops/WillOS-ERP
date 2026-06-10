using AutoMapper;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Branches.DTOs;
using EnterpriseERP.Domain.Entities.Settings;
using MediatR;

namespace EnterpriseERP.Application.Features.Branches.Commands.CreateBranch;

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, BranchDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBranchCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BranchDto> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Address = request.Address ?? string.Empty,
            Phone = request.Phone ?? string.Empty,
            IsActive = true
        };

        await _unitOfWork.Repository<Branch>().AddAsync(branch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BranchDto
        {
            Id = branch.Id,
            Name = branch.Name,
            Address = branch.Address,
            Phone = branch.Phone,
            IsActive = branch.IsActive
        };
    }
}
