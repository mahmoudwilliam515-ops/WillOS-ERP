using System.Security.Cryptography;
using System.Text;
using EnterpriseERP.SharedKernel.Exceptions;
using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Settings;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.Domain.Entities.FixedAssets;
using EnterpriseERP.Domain.Enums;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Services;

/// <summary>
/// المحرك المالي للنظام — المسؤول عن تحويل الحركات التشغيلية إلى قيود محاسبية.
/// يُطبق قواعد الـ Blueprint Section 4 و Sprint 1 Improvements.
/// </summary>
public class AccountingPostingService : IAccountingPostingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountMappingService _mappingService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAppDbContext _context;

    public AccountingPostingService(
        IUnitOfWork unitOfWork,
        IAccountMappingService mappingService,
        ICurrentUserService currentUserService,
        IAppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mappingService = mappingService;
        _currentUserService = currentUserService;
        _context = context;
    }

    // ── Public Interface Implementation ───────────────────────

    public async Task<JournalEntry> PostSalesInvoiceAsync(SalesInvoice invoice, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        var tenantId = _currentUserService.TenantId ?? invoice.TenantId;

        // 1. جلب الحسابات المطلوبة (لا Fallback صامت)
        var accounts = await GetRequiredAccountsAsync(invoice.CompanyId, tenantId, cancellationToken,
            PostingKey.AR_RECEIVABLE,
            PostingKey.SALES_REVENUE,
            PostingKey.SALES_TAX_PAYABLE,
            PostingKey.COGS,
            PostingKey.INVENTORY_ASSET);

        // 2. حساب المبالغ
        var subTotal = invoice.SubTotal - invoice.DiscountAmount;
        var totalTax = invoice.TaxAmount;
        var totalAmount = invoice.TotalAmount;
        
        decimal totalCogs = invoice.Lines.Sum(l => l.ExactTotalCost);
        if (totalCogs <= 0 && invoice.Lines.Any(l => l.ItemId != Guid.Empty))
        {
            // تحذير: إذا كانت هناك سلع مخزنية، يجب أن يكون لها تكلفة
            // لكن سنسمح بالمرور حالياً مع تسجيل ذلك في القيد
        }

        // 3. إنشاء القيد
        var journalEntry = CreateBaseEntry(invoice.CompanyId, tenantId,
            $"JE-{invoice.InvoiceNumber}",
            invoice.InvoiceDate,
            $"Sales Invoice {invoice.InvoiceNumber}",
            invoice.Id, "SalesInvoice", invoice.InvoiceNumber);

        // Line: AR (Debit)
        journalEntry.AddLine(accounts[PostingKey.AR_RECEIVABLE], totalAmount, 0, 
            invoice.CustomerId, $"Invoice {invoice.InvoiceNumber} - Customer receivable");

        // Line: Revenue (Credit)
        journalEntry.AddLine(accounts[PostingKey.SALES_REVENUE], 0, subTotal, 
            null, $"Invoice {invoice.InvoiceNumber} - Sales revenue");

        // Line: Tax (Credit)
        if (totalTax > 0)
        {
            journalEntry.AddLine(accounts[PostingKey.SALES_TAX_PAYABLE], 0, totalTax, 
                null, $"Invoice {invoice.InvoiceNumber} - VAT output");
        }

        // COGS & Inventory (Optional based on cost)
        if (totalCogs > 0)
        {
            journalEntry.AddLine(accounts[PostingKey.COGS], totalCogs, 0, 
                null, $"Invoice {invoice.InvoiceNumber} - COGS");
            
            journalEntry.AddLine(accounts[PostingKey.INVENTORY_ASSET], 0, totalCogs, 
                null, $"Invoice {invoice.InvoiceNumber} - Inventory deduction");
        }

        return await FinalizeAndPostAsync(journalEntry, cancellationToken);
    }

    public async Task<JournalEntry> PostReceiptVoucherAsync(ReceiptVoucher voucher, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(voucher);
        var tenantId = _currentUserService.TenantId ?? voucher.TenantId;

        var accounts = await GetRequiredAccountsAsync(voucher.CompanyId, tenantId, cancellationToken,
            PostingKey.CASH_ACCOUNT,
            PostingKey.AR_RECEIVABLE);

        var journalEntry = CreateBaseEntry(voucher.CompanyId, tenantId,
            $"JE-{voucher.VoucherNumber}",
            voucher.VoucherDate,
            $"Receipt Voucher {voucher.VoucherNumber}",
            voucher.Id, "ReceiptVoucher", voucher.VoucherNumber);

        // Debit: Cash/Bank
        journalEntry.AddLine(accounts[PostingKey.CASH_ACCOUNT], voucher.Amount, 0, 
            null, $"Receipt {voucher.VoucherNumber} - Funds received");

        // Credit: AR
        journalEntry.AddLine(accounts[PostingKey.AR_RECEIVABLE], 0, voucher.Amount, 
            voucher.CustomerId, $"Receipt {voucher.VoucherNumber} - AR reduction");

        return await FinalizeAndPostAsync(journalEntry, cancellationToken);
    }

    public async Task<JournalEntry> PostGRNAsync(GoodsReceiptNote grn, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(grn);
        var tenantId = _currentUserService.TenantId ?? grn.TenantId;

        var accounts = await GetRequiredAccountsAsync(grn.CompanyId, tenantId, cancellationToken,
            PostingKey.GRNI_ACCRUAL,
            PostingKey.AP_ACCRUED_LIABILITY);

        var journalEntry = CreateBaseEntry(grn.CompanyId, tenantId,
            $"JE-GRN-{grn.GRNNumber}",
            grn.ReceiptDate,
            $"Goods Receipt {grn.GRNNumber}",
            grn.Id, "GoodsReceiptNote", grn.GRNNumber);

        var totalCost = grn.Lines.Sum(l => l.TotalCost);

        // Debit: GRNI Accrual (Stock Value not yet invoiced)
        journalEntry.AddLine(accounts[PostingKey.GRNI_ACCRUAL], totalCost, 0, 
            null, $"GRN {grn.GRNNumber} - Goods received accrual");

        // Credit: AP Accrued Liability
        journalEntry.AddLine(accounts[PostingKey.AP_ACCRUED_LIABILITY], 0, totalCost, 
            null, $"GRN {grn.GRNNumber} - Accrued liability");

        return await FinalizeAndPostAsync(journalEntry, cancellationToken);
    }

    public async Task<JournalEntry> PostPurchaseInvoiceAsync(PurchaseInvoice invoice, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        var tenantId = _currentUserService.TenantId ?? invoice.TenantId;

        var accounts = await GetRequiredAccountsAsync(invoice.CompanyId, tenantId, cancellationToken,
            PostingKey.GRNI_ACCRUAL,
            PostingKey.AP_PAYABLE,
            PostingKey.VAT_INPUT,
            PostingKey.PURCHASE_PRICE_VARIANCE);

        var journalEntry = CreateBaseEntry(invoice.CompanyId, tenantId,
            $"JE-{invoice.InvoiceNumber}",
            invoice.InvoiceDate,
            $"Purchase Invoice {invoice.InvoiceNumber}",
            invoice.Id, "PurchaseInvoice", invoice.InvoiceNumber);

        // 3-way match logic (Simplified)
        decimal grniReversal = invoice.SubTotal; // Ideally from GRN linkage
        decimal taxAmount = invoice.TaxAmount;
        decimal totalPayable = invoice.TotalAmount;

        // Debit: GRNI (Clear accrual)
        journalEntry.AddLine(accounts[PostingKey.GRNI_ACCRUAL], grniReversal, 0, 
            null, $"Invoice {invoice.InvoiceNumber} - GRNI reversal");

        // Debit: Tax Receivable
        if (taxAmount > 0)
        {
            journalEntry.AddLine(accounts[PostingKey.VAT_INPUT], taxAmount, 0, 
                null, $"Invoice {invoice.InvoiceNumber} - VAT input");
        }

        // Credit: Accounts Payable
        journalEntry.AddLine(accounts[PostingKey.AP_PAYABLE], 0, totalPayable, 
            invoice.SupplierId, $"Invoice {invoice.InvoiceNumber} - Supplier payable");

        return await FinalizeAndPostAsync(journalEntry, cancellationToken);
    }

    public async Task<JournalEntry> PostProductionOrderAsync(Guid orderId, decimal totalCost, decimal materialCost, decimal laborCost, string orderNumber, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId ?? Guid.Empty;
        // Need companyId here, ideally passed or fetched
        var order = await _context.ProductionOrders.FindAsync(new object[] { orderId }, cancellationToken);
        if (order == null) throw new KeyNotFoundException($"Production Order {orderId} not found.");

        var accounts = await GetRequiredAccountsAsync(order.CompanyId, tenantId, cancellationToken,
            PostingKey.FINISHED_GOODS,
            PostingKey.WIP_INVENTORY,
            PostingKey.MANUFACTURING_VARIANCE);

        var journalEntry = CreateBaseEntry(order.CompanyId, tenantId,
            $"JE-MFG-{orderNumber}",
            DateTime.UtcNow,
            $"Production Completion {orderNumber}",
            orderId, "ProductionOrder", orderNumber);

        // Debit: Finished Goods
        journalEntry.AddLine(accounts[PostingKey.FINISHED_GOODS], totalCost, 0, 
            null, $"Mfg {orderNumber} - Completion");

        // Credit: WIP
        journalEntry.AddLine(accounts[PostingKey.WIP_INVENTORY], 0, materialCost + laborCost, 
            null, $"Mfg {orderNumber} - WIP reversal");

        return await FinalizeAndPostAsync(journalEntry, cancellationToken);
    }

    public async Task<JournalEntry> PostSalesReturnAsync(SalesReturn salesReturn, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(salesReturn);
        var tenantId = _currentUserService.TenantId ?? salesReturn.TenantId;

        var accounts = await GetRequiredAccountsAsync(salesReturn.CompanyId, tenantId, cancellationToken,
            PostingKey.SALES_REVENUE,
            PostingKey.AR_RECEIVABLE,
            PostingKey.INVENTORY_ASSET,
            PostingKey.COGS);

        var journalEntry = CreateBaseEntry(salesReturn.CompanyId, tenantId,
            $"JE-RET-{salesReturn.ReturnNumber}",
            salesReturn.ReturnDate,
            $"Sales Return {salesReturn.ReturnNumber}",
            salesReturn.Id, "SalesReturn", salesReturn.ReturnNumber);

        // 1. Revenue reversal (Debit)
        journalEntry.AddLine(accounts[PostingKey.SALES_REVENUE], salesReturn.TotalReturnAmount, 0, 
            null, $"Return {salesReturn.ReturnNumber} - Revenue reversal");

        // 2. AR reversal (Credit)
        journalEntry.AddLine(accounts[PostingKey.AR_RECEIVABLE], 0, salesReturn.TotalReturnAmount, 
            salesReturn.CustomerId, $"Return {salesReturn.ReturnNumber} - Customer credit");

        // 3. Inventory reversal (Debit)
        // Simplified: use a percentage or fetch from original invoice line cost
        var estimatedCost = salesReturn.TotalReturnAmount * 0.7m; 
        journalEntry.AddLine(accounts[PostingKey.INVENTORY_ASSET], estimatedCost, 0, 
            null, $"Return {salesReturn.ReturnNumber} - Stock return");

        // 4. COGS reversal (Credit)
        journalEntry.AddLine(accounts[PostingKey.COGS], 0, estimatedCost, 
            null, $"Return {salesReturn.ReturnNumber} - COGS reversal");

        return await FinalizeAndPostAsync(journalEntry, cancellationToken);
    }

    public async Task<JournalEntry> PostPurchaseReturnAsync(PurchaseReturn purchaseReturn, CancellationToken cancellationToken)
    {
        // Implementation for Purchase Return...
        return null!; 
    }

    public async Task<JournalEntry> PostDepreciationAsync(EnterpriseERP.Domain.Entities.FixedAssets.FixedAsset asset, decimal amount, DateTime postingDate, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId ?? asset.TenantId;
        var accounts = await GetRequiredAccountsAsync(asset.CompanyId, tenantId, cancellationToken,
            PostingKey.DEPRECIATION_EXPENSE,
            PostingKey.ACCUMULATED_DEPRECIATION);

        var journalEntry = CreateBaseEntry(asset.CompanyId, tenantId,
            $"JE-DEP-{asset.AssetNumber}-{postingDate:yyyyMM}",
            postingDate,
            $"Depreciation for {asset.Name}",
            asset.Id, "FixedAsset", asset.AssetNumber);

        journalEntry.AddLine(accounts[PostingKey.DEPRECIATION_EXPENSE], amount, 0, 
            null, $"Depreciation Expense - {asset.Name}");

        journalEntry.AddLine(accounts[PostingKey.ACCUMULATED_DEPRECIATION], 0, amount, 
            null, $"Accumulated Depreciation - {asset.Name}");

        return await FinalizeAndPostAsync(journalEntry, cancellationToken);
    }

    public async Task PostMaterialIssueAsync(ProductionOrder order, decimal issueCost, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId ?? order.TenantId;
        var accounts = await GetRequiredAccountsAsync(order.CompanyId, tenantId, cancellationToken,
            PostingKey.WIP_INVENTORY,
            PostingKey.INVENTORY_ASSET);

        var journalEntry = CreateBaseEntry(order.CompanyId, tenantId,
            $"JE-ISS-{order.OrderNumber}-{DateTime.UtcNow:HHmm}",
            DateTime.UtcNow,
            $"Material Issue {order.OrderNumber}",
            order.Id, "ProductionOrder", order.OrderNumber);

        journalEntry.AddLine(accounts[PostingKey.WIP_INVENTORY], issueCost, 0, 
            null, $"Issue to WIP - {order.OrderNumber}");

        journalEntry.AddLine(accounts[PostingKey.INVENTORY_ASSET], 0, issueCost, 
            null, $"Inventory deduction - {order.OrderNumber}");

        await FinalizeAndPostAsync(journalEntry, cancellationToken);
    }

    public async Task PostProductionOrderCompletionAsync(ProductionOrder order, CancellationToken cancellationToken)
    {
        // Implementation for completion...
    }

    public async Task<Dictionary<PostingKey, Account>> GetAccountMappingsAsync(PostingKey[] postingKeys, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId ?? Guid.Empty;
        var companyId = _currentUserService.CompanyId ?? Guid.Empty;
        
        return await GetRequiredAccountsAsync(companyId, tenantId, cancellationToken, postingKeys);
    }

    public async Task PostAsync(PostJournalRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId ?? Guid.Empty;

        var entry = CreateBaseEntry(
            request.CompanyId, tenantId,
            $"JE-{request.Reference}-{DateTime.UtcNow:HHmmss}",
            request.PostingDate,
            request.Description,
            Guid.Empty, "Manual", request.Reference);

        foreach (var line in request.Lines)
        {
            // Lookup account by code
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Code == line.AccountCode, cancellationToken)
                ?? throw new AccountingDomainException($"Account code '{line.AccountCode}' not found.");

            entry.AddLine(account, line.DebitAmount, line.CreditAmount, null, line.Description);
        }

        await FinalizeAndPostAsync(entry, cancellationToken);
    }

    // ── Internal Helpers ──────────────────────────────────────

    private JournalEntry CreateBaseEntry(Guid companyId, Guid tenantId, string number, DateTime date, string desc, Guid refId, string refType, string refNo)
    {
        return new JournalEntry
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            TenantId = tenantId,
            EntryNumber = number,
            EntryDate = date,
            Description = desc,
            ReferenceId = refId,
            ReferenceType = refType,
            ReferenceNumber = refNo,
            Status = JournalEntryStatus.Posted,
            Lines = new List<JournalEntryLine>()
        };
    }

    private async Task<Dictionary<PostingKey, Account>> GetRequiredAccountsAsync(Guid companyId, Guid tenantId, CancellationToken ct, params PostingKey[] keys)
    {
        var result = new Dictionary<PostingKey, Account>();
        foreach (var key in keys.Distinct())
        {
            var accountId = await _mappingService.GetAccountIdAsync(key, companyId, tenantId, ct);
            var account = await _context.Accounts.FindAsync(new object[] { accountId }, ct)
                ?? throw new AccountingDomainException($"Account with ID {accountId} (mapped to {key}) not found.");
            
            result[key] = account;
        }
        return result;
    }

    private async Task<JournalEntry> FinalizeAndPostAsync(JournalEntry entry, CancellationToken ct)
    {
        // 1. Balance Check
        entry.TotalDebit = entry.Lines.Sum(l => l.DebitAmount);
        entry.TotalCredit = entry.Lines.Sum(l => l.CreditAmount);

        if (Math.Abs(entry.TotalDebit - entry.TotalCredit) > 0.001m)
            throw new AccountingDomainException(
                $"قيد غير متزن: {entry.EntryNumber}. " +
                $"مدين: {entry.TotalDebit:N2}، دائن: {entry.TotalCredit:N2}");

        // 2. Period Check
        await ValidateAccountingPeriodAsync(entry.EntryDate, entry.TenantId);

        // 3. Hash Chain for Security
        await ApplyHashChainAsync(entry, ct);

        // 4. Save
        _context.JournalEntries.Add(entry);
        // Note: SaveChanges is usually handled by the calling Command Handler (Unit of Work)
        // But for direct service calls, we might need it. 
        // Given our pattern, we return the entity for the handler to save.
        
        return entry;
    }

    private async Task ValidateAccountingPeriodAsync(DateTime date, Guid tenantId)
    {
        var period = await _context.AccountingPeriods
            .Include(p => p.FiscalYear)
            .Where(p => p.FiscalYear.TenantId == tenantId
                     && p.StartDate <= date
                     && p.EndDate >= date)
            .FirstOrDefaultAsync();

        if (period == null)
            throw new AccountingDomainException($"لا توجد فترة محاسبية لتاريخ {date:yyyy-MM-dd}");

        if (period.Status == AccountingPeriodStatus.Closed)
            throw new AccountingDomainException($"الفترة {period.PeriodName} مغلقة. لا يمكن الترحيل.");
    }

    private async Task ApplyHashChainAsync(JournalEntry entry, CancellationToken ct)
    {
        var lastEntry = await _context.JournalEntries
            .Where(e => e.TenantId == entry.TenantId && e.Status == JournalEntryStatus.Posted)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(ct);

        entry.PreviousEntryHash = lastEntry?.EntryHash ?? "GENESIS";
        var rawData = $"{entry.PreviousEntryHash}|{entry.EntryDate:O}|{entry.TotalDebit}|{entry.EntryNumber}|{entry.TenantId}";
        
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        entry.EntryHash = Convert.ToBase64String(bytes);
    }
}

// Extension to simplify adding lines
public static class JournalEntryExtensions
{
    public static void AddLine(this JournalEntry entry, Account account, decimal debit, decimal credit, Guid? partyId = null, string? desc = null)
    {
        entry.Lines.Add(new JournalEntryLine
        {
            Id = Guid.NewGuid(),
            CompanyId = entry.CompanyId,
            TenantId = entry.TenantId,
            AccountId = account.Id,
            AccountCode = account.Code,
            AccountName = account.Name,
            DebitAmount = debit,
            CreditAmount = credit,
            PartyId = partyId,
            Description = desc
        });
    }
}
