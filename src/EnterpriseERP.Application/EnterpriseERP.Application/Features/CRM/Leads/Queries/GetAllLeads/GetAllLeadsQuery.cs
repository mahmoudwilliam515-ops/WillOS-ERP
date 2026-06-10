using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.CRM;
using MediatR;

namespace EnterpriseERP.Application.Features.CRM.Leads.Queries.GetAllLeads;

public record GetAllLeadsQuery(int Page = 1, int PageSize = 20) : IRequest<GetAllLeadsResult>;

public class GetAllLeadsResult
{
    public List<LeadDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class LeadDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal EstimatedValue { get; set; }
}

public class GetAllLeadsQueryHandler : IRequestHandler<GetAllLeadsQuery, GetAllLeadsResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllLeadsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetAllLeadsResult> Handle(GetAllLeadsQuery request, CancellationToken cancellationToken)
    {
        var all = await _unitOfWork.Repository<Lead>().FindAsync(x => true);
        var ordered = all.OrderByDescending(x => x.CreatedAt).ToList();
        var total = ordered.Count;
        var paged = ordered.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);

        return new GetAllLeadsResult
        {
            TotalCount = total,
            Items = paged.Select(l => new LeadDto
            {
                Id = l.Id,
                FullName = $"{l.FirstName} {l.LastName}",
                CompanyName = l.CompanyName,
                Email = l.Email,
                Status = l.Status.ToString(),
                EstimatedValue = l.EstimatedValue
            }).ToList()
        };
    }
}
