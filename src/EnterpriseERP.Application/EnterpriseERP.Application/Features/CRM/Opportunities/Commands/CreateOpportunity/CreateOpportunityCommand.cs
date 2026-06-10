using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.CRM;
using MediatR;

namespace EnterpriseERP.Application.Features.CRM.Opportunities.Commands.CreateOpportunity;

public class CreateOpportunityCommand : IRequest<Guid>
{
    public string Title { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public decimal ExpectedRevenue { get; set; }
    public double ProbabilityPercentage { get; set; }
    public DateTime ExpectedCloseDate { get; set; }
}

public class CreateOpportunityCommandHandler : IRequestHandler<CreateOpportunityCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateOpportunityCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var opp = new Opportunity
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            CustomerId = request.CustomerId,
            ExpectedRevenue = request.ExpectedRevenue,
            ProbabilityPercentage = request.ProbabilityPercentage,
            ExpectedCloseDate = request.ExpectedCloseDate,
            Stage = "Prospecting"
        };

        await _unitOfWork.Repository<Opportunity>().AddAsync(opp);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return opp.Id;
    }
}
