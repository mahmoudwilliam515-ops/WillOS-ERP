using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.ApproveGRN;

public record ApproveGRNCommand : IRequest<Result>
{
    public Guid GRNId { get; init; }
    public Guid CompanyId { get; init; }
    public Guid ApprovedByUserId { get; init; }
}

public class ApproveGRNCommandHandler : IRequestHandler<ApproveGRNCommand, Result>
{
    private readonly IAppDbContext _context;
    private readonly IAccountMappingService _accountMappingService;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IInventoryService _inventoryService;
    private readonly IPeriodClosingService _periodService;

    public ApproveGRNCommandHandler(
        IAppDbContext context,
        IAccountMappingService accountMappingService,
        IAccountingPostingService accountingPostingService,
        IInventoryService inventoryService,
        IPeriodClosingService periodService)
    {
        _context = context;
        _accountMappingService = accountMappingService;
        _accountingPostingService = accountingPostingService;
        _inventoryService = inventoryService;
        _periodService = periodService;
    }

    public async Task<Result> Handle(ApproveGRNCommand command, CancellationToken cancellationToken)
    {
        var grn = await _context.GoodsReceiptNotes
            .Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == command.GRNId && g.CompanyId == command.CompanyId, cancellationToken);

        if (grn == null)
            return Result.Failure(new Error("Procurement.GRNNotFound", $"GRN {command.GRNId} not found for company {command.CompanyId}"));

        // 1. فحص الفترة المحاسبية
        var periodResult = await _periodService.IsOpenAsync(grn.ReceiptDate, cancellationToken);
        if (!periodResult)
            return Result.Failure(new Error("Accounting.PeriodClosed", "Accounting period is closed for the selected date."));

        // 2. اعتماد الكيان (يطلق Domain Event)
        try 
        {
            grn.Approve(command.ApprovedByUserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("Procurement.InvalidGRNStatus", ex.Message));
        }

        // 3. GRNI Accrual Posting
        // Dr: GRNI Accrual (الأصول المستلمة غير المفوترة بعد)
        // Cr: AP Accrued Liability (استحقاق الالتزام تجاه المورد)
        var totalCost = grn.Lines.Sum(l => l.TotalCost);
        if (totalCost <= 0)
            return Result.Failure(new Error("Procurement.ZeroTotalCost", $"GRN {grn.GRNNumber} has zero or negative total cost. Cannot post GRNI accrual."));

        var postingResult = await _accountingPostingService.PostGRNAsync(grn, cancellationToken);
        // Wait, PostGRNAsync returns JournalEntry, not Result. Let's just catch exceptions.
        
        // 4. تحديث المخزون
        foreach (var line in grn.Lines)
        {
            await _inventoryService.IncreaseStockAsync(
                line.ItemId,
                grn.WarehouseId,
                command.CompanyId,
                line.ReceivedQuantity,
                line.UnitCost,
                $"GRN-{grn.GRNNumber}",
                cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
