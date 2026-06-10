using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.Domain.Entities.Sales;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.SalesReturns.Commands.CreateSalesReturn;

public class CreateSalesReturnCommandHandler : IRequestHandler<CreateSalesReturnCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IOrderNumberGenerator _numberingService;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IInventoryPostingService _inventoryService;
    private readonly IPeriodClosingService _periodService;
    private readonly ICurrentUserService _currentUserService;

    public CreateSalesReturnCommandHandler(
        IAppDbContext context,
        IOrderNumberGenerator numberingService,
        IAccountingPostingService accountingPostingService,
        IInventoryPostingService inventoryService,
        IPeriodClosingService periodService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _numberingService = numberingService;
        _accountingPostingService = accountingPostingService;
        _inventoryService = inventoryService;
        _periodService = periodService;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateSalesReturnCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId ?? Guid.Empty;

        // 1. فحص الفترة المحاسبية
        await _periodService.ValidateOpenAsync(command.ReturnDate, cancellationToken);

        // 2. التحقق من الفاتورة الأصلية
        var originalInvoice = await _context.SalesInvoices
            .FirstOrDefaultAsync(i => i.Id == command.OriginalInvoiceId 
                                   && i.CompanyId == command.CompanyId
                                   && i.TenantId == tenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Original invoice {command.OriginalInvoiceId} not found");

        // 3. توليد رقم المرتجع (Credit Note Number)
        var returnNumber = await _numberingService.GenerateAsync("CR", command.CompanyId, tenantId, cancellationToken);

        // 4. إنشاء الكيان
        var salesReturn = SalesReturn.Create(
            command.CompanyId,
            command.OriginalInvoiceId,
            originalInvoice.CustomerId,
            command.ReturnDate,
            returnNumber,
            command.ReturnReason);
        
        salesReturn.TenantId = tenantId;

        foreach (var line in command.Lines)
        {
            salesReturn.AddLine(line.ItemId, line.ReturnQuantity, line.UnitPrice);
        }

        // 5. اعتماد المرتجع
        salesReturn.Approve();

        // 6. عكس القيود المحاسبية (Revenue, AR, Inventory, COGS)
        await _accountingPostingService.PostSalesReturnAsync(salesReturn, cancellationToken);

        // 7. إعادة البضاعة للمخزون (Inventory Posting)
        await _inventoryService.PostSalesReturnAsync(salesReturn, cancellationToken);

        // 8. تحديث الرصيد المتبقي على الفاتورة الأصلية (تخفيض المديونية)
        // Note: Logic for updating invoice balance might be needed here if not handled by event
        originalInvoice.RemainingAmount -= salesReturn.TotalReturnAmount;
        if (originalInvoice.RemainingAmount < 0) originalInvoice.RemainingAmount = 0;

        _context.SalesReturns.Add(salesReturn);
        await _context.SaveChangesAsync(cancellationToken);

        return salesReturn.Id;
    }
}
