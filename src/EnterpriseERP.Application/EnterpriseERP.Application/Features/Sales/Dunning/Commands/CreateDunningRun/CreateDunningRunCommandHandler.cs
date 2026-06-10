using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Dunning.Commands.CreateDunningRun;

/// <summary>
/// محرك الإشعارات التلقائية للتأخر في السداد
/// Blueprint Section 1.2 — Dunning Engine
/// يُجدوَل يومياً أو أسبوعياً عبر Background Service
/// </summary>
public record CreateDunningRunCommand : IRequest<DunningRunResult>
{
    public Guid CompanyId { get; init; }
    public DateTime RunDate { get; init; }
    public Guid? ExecutedByUserId { get; init; }
}

public class CreateDunningRunCommandHandler : IRequestHandler<CreateDunningRunCommand, DunningRunResult>
{
    private readonly IAppDbContext _context;
    private readonly INotificationService _notificationService;

    // تعريف حزم التأخر (Aging Buckets)
    private static readonly DunningLevel[] Levels = new[]
    {
        new DunningLevel(MinDays: 1,   MaxDays: 30,  Level: 1, Message: "Friendly reminder: Invoice overdue"),
        new DunningLevel(MinDays: 31,  MaxDays: 60,  Level: 2, Message: "Second notice: Payment required"),
        new DunningLevel(MinDays: 61,  MaxDays: 90,  Level: 3, Message: "Final notice before collections"),
        new DunningLevel(MinDays: 91,  MaxDays: null, Level: 4, Message: "Referred to collections department"),
    };

    public CreateDunningRunCommandHandler(
        IAppDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<DunningRunResult> Handle(CreateDunningRunCommand command, CancellationToken cancellationToken)
    {
        // جلب الفواتير المتأخرة
        var overdueInvoices = await _context.SalesInvoices
            .Include(i => i.Customer)
            .Where(i => i.CompanyId == command.CompanyId
                     && i.Status != InvoiceStatus.Paid
                     && i.Status != InvoiceStatus.Cancelled
                     && i.DueDate < command.RunDate
                     && i.RemainingAmount > 0)
            .ToListAsync(cancellationToken);

        var noticesSent = new List<DunningNotice>();

        foreach (var invoice in overdueInvoices)
        {
            var daysOverdue = (int)(command.RunDate - invoice.DueDate).TotalDays;
            var level = GetDunningLevel(daysOverdue);

            if (level == null) continue;

            // تحقق — هل أُرسل إشعار بنفس المستوى مؤخراً؟
            var recentNotice = await _context.DunningNotices
                .AnyAsync(n => n.InvoiceId == invoice.Id
                             && n.Level == level.Level
                             && n.SentAt >= command.RunDate.AddDays(-7), cancellationToken);

            if (recentNotice) continue;

            var notice = new DunningNotice
            {
                CompanyId = command.CompanyId,
                InvoiceId = invoice.Id,
                CustomerId = invoice.CustomerId,
                Level = level.Level,
                DaysOverdue = daysOverdue,
                AmountDue = invoice.RemainingAmount,
                SentAt = command.RunDate,
                Message = level.Message
            };

            _context.DunningNotices.Add(notice);
            noticesSent.Add(notice);

            // إرسال الإشعار عبر النظام
            await _notificationService.SendSystemAlertAsync(
                title: $"Dunning Notice Level {level.Level}",
                message: $"{level.Message} — Invoice #{invoice.InvoiceNumber}, Amount: {invoice.RemainingAmount:N2}, Days Overdue: {daysOverdue}",
                severity: level.Level >= 3 ? "HIGH" : "MEDIUM");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new DunningRunResult
        {
            RunDate = command.RunDate,
            TotalOverdueInvoices = overdueInvoices.Count,
            NoticesSent = noticesSent.Count,
            TotalAmountOverdue = overdueInvoices.Sum(i => i.RemainingAmount)
        };
    }

    private static DunningLevel? GetDunningLevel(int daysOverdue)
    {
        return Levels.FirstOrDefault(l =>
            daysOverdue >= l.MinDays && (l.MaxDays == null || daysOverdue <= l.MaxDays));
    }
}

public record DunningLevel(int MinDays, int? MaxDays, int Level, string Message);

public class DunningRunResult
{
    public DateTime RunDate { get; init; }
    public int TotalOverdueInvoices { get; init; }
    public int NoticesSent { get; init; }
    public decimal TotalAmountOverdue { get; init; }
}
