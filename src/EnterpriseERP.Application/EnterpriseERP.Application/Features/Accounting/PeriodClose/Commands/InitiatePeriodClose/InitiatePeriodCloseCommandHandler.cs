using EnterpriseERP.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.PeriodClose.Commands;

// ── Initiate Period Close ──────────────────────────────────────────────────────

public record InitiatePeriodCloseCommand : IRequest<Guid>
{
    public Guid CompanyId { get; init; }
    public Guid PeriodId { get; init; }
    public Guid InitiatedByUserId { get; init; }
}

public class InitiatePeriodCloseCommandHandler : IRequestHandler<InitiatePeriodCloseCommand, Guid>
{
    private readonly IAppDbContext _context;

    public InitiatePeriodCloseCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(
        InitiatePeriodCloseCommand request,
        CancellationToken cancellationToken)
    {
        var period = await _context.AccountingPeriods
            .Where(p => p.Id == request.PeriodId && p.CompanyId == request.CompanyId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"AccountingPeriod {request.PeriodId} not found");

        if (period.Status == EnterpriseERP.Domain.Entities.Accounting.AccountingPeriodStatus.Closed)
            throw new InvalidOperationException($"Period '{period.PeriodName}' is already closed");

        // إنشاء Checklist بالخطوات القياسية (الخطوات تُضاف تلقائياً في Create)
        var checklist = EnterpriseERP.Domain.Reporting.PeriodCloseChecklist.Create(
            request.CompanyId,
            request.PeriodId,
            period.PeriodName);

        await _context.PeriodCloseChecklists.AddAsync(checklist, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return checklist.Id;
    }
}

// ── Approve (Lock) Period ─────────────────────────────────────────────────────

public record ApprovePeriodCloseCommand : IRequest<Unit>
{
    public Guid CompanyId { get; init; }
    public Guid ChecklistId { get; init; }
    public Guid ApprovedByUserId { get; init; }
}

public class ApprovePeriodCloseCommandHandler : IRequestHandler<ApprovePeriodCloseCommand, Unit>
{
    private readonly IAppDbContext _context;

    public ApprovePeriodCloseCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        ApprovePeriodCloseCommand request,
        CancellationToken cancellationToken)
    {
        var checklist = await _context.PeriodCloseChecklists
            .Include(c => c.Steps)
            .Where(c => c.Id == request.ChecklistId && c.CompanyId == request.CompanyId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"PeriodCloseChecklist {request.ChecklistId} not found");

        // التحقق أن كل الخطوات مكتملة
        var incompleteSteps = checklist.Steps
            .Where(s => !s.IsSignedOff)
            .Select(s => s.Code)
            .ToList();

        if (incompleteSteps.Any())
            throw new InvalidOperationException(
                $"Cannot approve period close: incomplete steps: {string.Join(", ", incompleteSteps)}");

        // قفل الفترة
        var period = await _context.AccountingPeriods
            .Where(p => p.Id == checklist.FiscalPeriodId && p.CompanyId == request.CompanyId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("AccountingPeriod not found");

        period.Status = EnterpriseERP.Domain.Entities.Accounting.AccountingPeriodStatus.Closed;
        period.ClosedBy = request.ApprovedByUserId.ToString();
        period.ClosedAt = DateTime.UtcNow;
        checklist.LockPeriod(request.ApprovedByUserId);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
