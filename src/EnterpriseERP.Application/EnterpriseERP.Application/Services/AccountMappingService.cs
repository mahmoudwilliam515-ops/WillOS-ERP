using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Enums;
using EnterpriseERP.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EnterpriseERP.Application.Services;

public class AccountMappingService : IAccountMappingService
{
    private readonly IAppDbContext _context;
    private readonly IMemoryCache _cache;

    public AccountMappingService(IAppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Guid> GetAccountIdAsync(
        PostingKey postingKey,
        Guid companyId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        // Cache Key
        var cacheKey = $"AccountMapping:{tenantId}:{companyId}:{postingKey}";

        if (_cache.TryGetValue(cacheKey, out Guid cachedAccountId))
            return cachedAccountId;

        var mapping = await _context.AccountMappings
            .Where(am => am.PostingKey == postingKey
                      && am.CompanyId == companyId
                      && am.TenantId == tenantId
                      && am.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (mapping is null)
            throw new AccountMappingNotFoundException(
                $"لا يوجد AccountMapping لـ PostingKey={postingKey} " +
                $"في الشركة {companyId}. " +
                $"يرجى إعداد الـ Chart of Accounts Mapping في الإعدادات المالية.");

        // Cache لمدة 5 دقائق
        _cache.Set(cacheKey, mapping.AccountId,
            TimeSpan.FromMinutes(5));

        return mapping.AccountId;
    }

    public async Task<string> GetAccountCodeAsync(
        AccountMappingKey key,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        // Map AccountMappingKey to a PostingKey or directly query by account code category
        var cacheKey = $"AccountCode:{companyId}:{key}";

        if (_cache.TryGetValue(cacheKey, out string? cachedCode) && cachedCode is not null)
            return cachedCode;

        // Map AccountMappingKey to PostingKey
        var postingKey = key switch
        {
            AccountMappingKey.AccountsReceivable => PostingKey.AR_RECEIVABLE,
            AccountMappingKey.AccountsPayable => PostingKey.AP_PAYABLE,
            AccountMappingKey.SalesRevenue => PostingKey.SALES_REVENUE,
            AccountMappingKey.COGS => PostingKey.COGS,
            AccountMappingKey.Inventory => PostingKey.INVENTORY_ASSET,
            AccountMappingKey.GRNI => PostingKey.GRNI_ACCRUAL,
            AccountMappingKey.CashAndBank => PostingKey.CASH_ACCOUNT,
            AccountMappingKey.TaxPayable => PostingKey.VAT_OUTPUT,
            AccountMappingKey.SalesReturnExpense => PostingKey.SALES_RETURN,
            AccountMappingKey.PurchaseReturnIncome => PostingKey.AP_PAYABLE,
            AccountMappingKey.PriceVariance => PostingKey.PURCHASE_PRICE_VARIANCE,
            AccountMappingKey.FXGainLoss => PostingKey.FX_GAIN,
            _ => throw new AccountMappingNotFoundException($"Unsupported AccountMappingKey: {key}")
        };

        var mapping = await _context.AccountMappings
            .Where(am => am.CompanyId == companyId
                      && am.IsActive
                      && am.PostingKey == postingKey)
            .FirstOrDefaultAsync(cancellationToken);

        if (mapping is null)
            throw new AccountMappingNotFoundException(
                $"No AccountMapping for key={key} in company {companyId}.");

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == mapping.AccountId, cancellationToken);

        var code = account?.Code ?? mapping.AccountId.ToString();
        _cache.Set(cacheKey, code, TimeSpan.FromMinutes(5));
        return code;
    }

    public async Task<string> GetBankAccountGLCodeAsync(
        Guid bankAccountId,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"BankGLCode:{companyId}:{bankAccountId}";

        if (_cache.TryGetValue(cacheKey, out string? cachedCode) && cachedCode is not null)
            return cachedCode;

        var bankAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == bankAccountId && b.CompanyId == companyId, cancellationToken);

        if (bankAccount is null)
            throw new AccountMappingNotFoundException(
                $"BankAccount {bankAccountId} not found for company {companyId}.");

        // Return the GL code (AccountNumber serves as GL code)
        var code = bankAccount.AccountNumber;
        _cache.Set(cacheKey, code, TimeSpan.FromMinutes(5));
        return code;
    }
}
