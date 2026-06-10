using EnterpriseERP.Domain.Enums;
using EnterpriseERP.Application.Common.Interfaces.Services;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IAccountMappingService
{
    /// <summary>
    /// يُرجع الـ AccountId للـ PostingKey المطلوب.
    /// يرمي AccountMappingNotFoundException إذا لم يوجد.
    /// لا Fallback صامت أبداً.
    /// </summary>
    Task<Guid> GetAccountIdAsync(
        PostingKey postingKey,
        Guid companyId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>Get GL account code by mapping key</summary>
    Task<string> GetAccountCodeAsync(
        AccountMappingKey key,
        Guid companyId,
        CancellationToken cancellationToken = default);

    /// <summary>Get GL account code for a specific bank account</summary>
    Task<string> GetBankAccountGLCodeAsync(
        Guid bankAccountId,
        Guid companyId,
        CancellationToken cancellationToken = default);
}

public enum AccountMappingKey
{
    AccountsReceivable,
    AccountsPayable,
    SalesRevenue,
    COGS,
    Inventory,
    GRNI,
    CashAndBank,
    TaxPayable,
    SalesReturnExpense,
    PurchaseReturnIncome,
    PriceVariance,
    FXGainLoss
}
