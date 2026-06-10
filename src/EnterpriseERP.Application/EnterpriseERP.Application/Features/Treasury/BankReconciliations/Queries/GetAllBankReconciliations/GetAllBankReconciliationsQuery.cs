using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Treasury;
using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.BankReconciliations.Queries.GetAllBankReconciliations;

public record GetAllBankReconciliationsQuery(int Page = 1, int PageSize = 20) : IRequest<GetAllBankReconciliationsResult>;

public class GetAllBankReconciliationsResult
{
    public List<BankReconciliationDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class BankReconciliationDto
{
    public Guid Id { get; set; }
    public Guid BankAccountId { get; set; }
    public DateTime StatementDate { get; set; }
    public decimal StatementEndingBalance { get; set; }
    public decimal ClearedBalance { get; set; }
    public bool IsClosed { get; set; }
}

public class GetAllBankReconciliationsQueryHandler : IRequestHandler<GetAllBankReconciliationsQuery, GetAllBankReconciliationsResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllBankReconciliationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetAllBankReconciliationsResult> Handle(GetAllBankReconciliationsQuery request, CancellationToken cancellationToken)
    {
        var all = await _unitOfWork.Repository<BankReconciliation>().FindAsync(x => true);
        var ordered = all.OrderByDescending(x => x.StatementDate).ToList();
        var total = ordered.Count;
        var paged = ordered.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);

        return new GetAllBankReconciliationsResult
        {
            TotalCount = total,
            Items = paged.Select(b => new BankReconciliationDto
            {
                Id = b.Id,
                BankAccountId = b.BankAccountId,
                StatementDate = b.StatementDate,
                StatementEndingBalance = b.StatementEndingBalance,
                ClearedBalance = b.ClearedBalance,
                IsClosed = b.IsClosed
            }).ToList()
        };
    }
}
