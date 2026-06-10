using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.CRM;
using MediatR;

namespace EnterpriseERP.Application.Features.CRM.Opportunities.Queries.GetAllOpportunities;

public record GetAllOpportunitiesQuery(int Page = 1, int PageSize = 20) : IRequest<GetAllOpportunitiesResult>;

public class GetAllOpportunitiesResult
{
    public List<OpportunityDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class OpportunityDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public decimal ExpectedRevenue { get; set; }
    public double ProbabilityPercentage { get; set; }
    public DateTime ExpectedCloseDate { get; set; }
    public string Stage { get; set; } = string.Empty;
}

public class GetAllOpportunitiesQueryHandler : IRequestHandler<GetAllOpportunitiesQuery, GetAllOpportunitiesResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllOpportunitiesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetAllOpportunitiesResult> Handle(GetAllOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        var all = await _unitOfWork.Repository<Opportunity>().FindAsync(x => true);
        var ordered = all.OrderByDescending(x => x.CreatedAt).ToList();
        var total = ordered.Count;
        var paged = ordered.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);

        return new GetAllOpportunitiesResult
        {
            TotalCount = total,
            Items = paged.Select(o => new OpportunityDto
            {
                Id = o.Id,
                Title = o.Title,
                CustomerId = o.CustomerId,
                ExpectedRevenue = o.ExpectedRevenue,
                ProbabilityPercentage = o.ProbabilityPercentage,
                ExpectedCloseDate = o.ExpectedCloseDate,
                Stage = o.Stage
            }).ToList()
        };
    }
}
