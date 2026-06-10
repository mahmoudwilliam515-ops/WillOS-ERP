using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Domain.Enums;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Domain.Sales;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IAccountingPostingService
{
    Task<JournalEntry> PostSalesInvoiceAsync(SalesInvoice invoice, CancellationToken cancellationToken);

    /// <summary>
    /// Generates and validates a JournalEntry for a Purchase Invoice.
    /// Double-entry validation (Debits == Credits) must be strictly enforced.
    /// </summary>
    Task<JournalEntry> PostPurchaseInvoiceAsync(PurchaseInvoice invoice, CancellationToken cancellationToken);
    Task<JournalEntry> PostGRNAsync(GoodsReceiptNote grn, CancellationToken cancellationToken);

    /// <summary>
    /// Generates and validates a JournalEntry for a completed Production Order.
    /// Double-entry validation (Debits == Credits) must be strictly enforced.
    /// </summary>
    Task<JournalEntry> PostProductionOrderAsync(Guid orderId, decimal totalCost, decimal materialCost, decimal laborCost, string orderNumber, CancellationToken cancellationToken);
    
    Task<JournalEntry> PostReceiptVoucherAsync(EnterpriseERP.Domain.Entities.Treasury.ReceiptVoucher voucher, CancellationToken cancellationToken);
    
    Task<JournalEntry> PostSalesReturnAsync(SalesReturn salesReturn, CancellationToken cancellationToken);

    Task<JournalEntry> PostPurchaseReturnAsync(PurchaseReturn purchaseReturn, CancellationToken cancellationToken);

    Task<JournalEntry> PostDepreciationAsync(EnterpriseERP.Domain.Entities.FixedAssets.FixedAsset asset, decimal amount, DateTime postingDate, CancellationToken cancellationToken);

    Task PostMaterialIssueAsync(ProductionOrder order, decimal issueCost, CancellationToken cancellationToken);
    Task PostProductionOrderCompletionAsync(ProductionOrder order, CancellationToken cancellationToken);
    
    Task<Dictionary<PostingKey, Account>> GetAccountMappingsAsync(PostingKey[] postingKeys, CancellationToken cancellationToken = default);

    // Unified posting method using PostJournalRequest
    Task PostAsync(PostJournalRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Unified journal posting request</summary>
public record PostJournalRequest(
    Guid CompanyId,
    Guid PeriodId,
    string Description,
    string Reference,
    DateTime PostingDate,
    IReadOnlyList<JournalLineRequest> Lines
);

public record JournalLineRequest(
    string AccountCode,
    decimal DebitAmount,
    decimal CreditAmount,
    string? Description = null
);
