using System.Linq;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Intercompany.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Settings;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Intercompany.Commands.RunConsolidation;

public class RunConsolidationCommandHandler
    : IRequestHandler<RunConsolidationCommand, ConsolidationRunDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public RunConsolidationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ConsolidationRunDto> Handle(RunConsolidationCommand request, CancellationToken cancellationToken)
    {
        var groupCompany = await _unitOfWork.Repository<Company>().GetByIdAsync(request.GroupCompanyId);
        if (groupCompany == null || !groupCompany.IsConsolidationEntity)
            throw new InvalidOperationException("Invalid consolidation entity.");

        var transactions = await _unitOfWork.Repository<IntercompanyTransaction>().FindAsync(t =>
            t.TransactionDate >= request.PeriodStart &&
            t.TransactionDate <= request.PeriodEnd);

        var intercompanyAmount = transactions.Sum(t => t.Amount);
        var eliminatedAmount = transactions.Where(t => t.Status == IntercompanyTransactionStatus.Matched).Sum(t => t.Amount);
        var unmatchedAmount = transactions.Where(t => t.Status != IntercompanyTransactionStatus.Matched).Sum(t => t.Amount);

        var run = new ConsolidationRun
        {
            Id = Guid.NewGuid(),
            RunNumber = $"CON-{DateTime.UtcNow:yyyyMMddHHmmss}",
            GroupCompanyId = request.GroupCompanyId,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            IntercompanyAmount = intercompanyAmount,
            EliminatedAmount = eliminatedAmount,
            UnmatchedAmount = unmatchedAmount,
            Status = ConsolidationRunStatus.Completed,
            CompletedAt = DateTime.UtcNow,
            Notes = "Automated consolidation run."
        };

        await _unitOfWork.Repository<ConsolidationRun>().AddAsync(run);

        foreach (var transaction in transactions.Where(t => t.Status == IntercompanyTransactionStatus.Matched))
        {
            transaction.Status = IntercompanyTransactionStatus.Eliminated;
            transaction.ConsolidationRunId = run.Id;
            _unitOfWork.Repository<IntercompanyTransaction>().Update(transaction);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ConsolidationRunDto
        {
            Id = run.Id,
            RunNumber = run.RunNumber,
            GroupCompanyId = run.GroupCompanyId,
            PeriodStart = run.PeriodStart,
            PeriodEnd = run.PeriodEnd,
            IntercompanyAmount = run.IntercompanyAmount,
            EliminatedAmount = run.EliminatedAmount,
            UnmatchedAmount = run.UnmatchedAmount,
            Status = run.Status,
            CompletedAt = run.CompletedAt,
            Notes = run.Notes
        };
    }
}
