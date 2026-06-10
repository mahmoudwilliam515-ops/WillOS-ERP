using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.CRM;
using MediatR;

namespace EnterpriseERP.Application.Features.CRM.Leads.Commands.CreateLead;

public class CreateLeadCommand : IRequest<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal EstimatedValue { get; set; }
}

public class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            CompanyName = request.CompanyName,
            Email = request.Email,
            Phone = request.Phone,
            EstimatedValue = request.EstimatedValue,
            Status = LeadStatus.New
        };

        await _unitOfWork.Repository<Lead>().AddAsync(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lead.Id;
    }
}
