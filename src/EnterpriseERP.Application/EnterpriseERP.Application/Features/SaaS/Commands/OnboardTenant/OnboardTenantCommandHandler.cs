using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.SaaS;
using EnterpriseERP.Domain.Entities.Identity;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Settings;

using MediatR;

namespace EnterpriseERP.Application.Features.SaaS.Commands.OnboardTenant;

public class OnboardTenantCommandHandler : IRequestHandler<OnboardTenantCommand, TenantOnboardingResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantOnboardingIdentityService _identityService;
    private readonly IEmailService _emailService;

    public OnboardTenantCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantOnboardingIdentityService identityService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _emailService = emailService;
    }

    public async Task<TenantOnboardingResult> Handle(OnboardTenantCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Create Tenant
            var tenant = new Tenant
            {
                Name = request.Name,
                SubDomain = request.SubDomain,
                Status = TenantStatus.Trial,
                SubscriptionPlanId = request.SubscriptionPlanId,
                SubscriptionStartDate = DateTime.UtcNow,
                SubscriptionEndDate = DateTime.UtcNow.AddDays(30), // 30-day trial
                MaxUsers = 10,
                DefaultCurrency = request.DefaultCurrency,
                DefaultLanguage = request.DefaultLanguage,
                TimeZone = request.TimeZone,
                FiscalYearStart = "01-01",
                TenantId = Guid.Empty // Will be set by SaveChangesAsync
            };

            await _unitOfWork.Repository<Tenant>().AddAsync(tenant);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 2, 3, 4. Create Admin User, Roles and assign Role
            var identityResult = await _identityService.CreateAdminUserAndRolesAsync(
                tenant.Id, 
                request.AdminEmail, 
                request.AdminFullName, 
                request.AdminPassword);

            // 5. Seed Default Chart of Accounts
            await SeedDefaultChartOfAccountsAsync(tenant.Id, request.CountryCode);

            // 6. Seed Default Account Mappings
            await SeedDefaultAccountMappingsAsync(tenant.Id);

            // 7. Seed Default Fiscal Year
            await SeedDefaultFiscalYearAsync(tenant.Id);

            await _unitOfWork.CommitTransactionAsync();

            // 8. Send Welcome Email
            // await _emailService.SendWelcomeEmail(adminUser.Email, tenant);

            return new TenantOnboardingResult(
                tenant.Id,
                identityResult.UserId,
                identityResult.Email,
                tenant.SubDomain
            );
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }



    private async Task SeedDefaultChartOfAccountsAsync(Guid tenantId, string countryCode)
    {
        /*
        // This is a simplified version - in production, this would load from a template based on countryCode
        var defaultAccounts = new[]
        {
            new Account { Code = "1000", Name = "Assets", AccountType = AccountType.Asset, TenantId = tenantId },
            new Account { Code = "1100", Name = "Cash", AccountType = AccountType.Asset, TenantId = tenantId },
            new Account { Code = "1200", Name = "Accounts Receivable", AccountType = AccountType.Asset, TenantId = tenantId },
            new Account { Code = "2000", Name = "Liabilities", AccountType = AccountType.Liability, TenantId = tenantId },
            new Account { Code = "2100", Name = "Accounts Payable", AccountType = AccountType.Liability, TenantId = tenantId },
            new Account { Code = "3000", Name = "Equity", AccountType = AccountType.Equity, TenantId = tenantId },
            new Account { Code = "4000", Name = "Revenue", AccountType = AccountType.Revenue, TenantId = tenantId },
            new Account { Code = "5000", Name = "Cost of Goods Sold", AccountType = AccountType.Expense, TenantId = tenantId },
            new Account { Code = "6000", Name = "Operating Expenses", AccountType = AccountType.Expense, TenantId = tenantId }
        };

        foreach (var account in defaultAccounts)
        {
            await _unitOfWork.Repository<Account>().AddAsync(account);
        }

        await _unitOfWork.SaveChangesAsync();
        */
        await Task.CompletedTask;
    }

    private async Task SeedDefaultAccountMappingsAsync(Guid tenantId)
    {
        /*
        var defaultMappings = new[]
        {
            new AccountMapping { PostingType = "AccountsReceivable", AccountCode = "1200", TenantId = tenantId },
            new AccountMapping { PostingType = "SalesRevenue", AccountCode = "4000", TenantId = tenantId },
            new AccountMapping { PostingType = "TaxPayable", AccountCode = "2100", TenantId = tenantId },
            new AccountMapping { PostingType = "COGS", AccountCode = "5000", TenantId = tenantId },
            new AccountMapping { PostingType = "Inventory", AccountCode = "1100", TenantId = tenantId },
            new AccountMapping { PostingType = "AccountsPayable", AccountCode = "2100", TenantId = tenantId },
            new AccountMapping { PostingType = "Cash", AccountCode = "1100", TenantId = tenantId }
        };

        foreach (var mapping in defaultMappings)
        {
            await _unitOfWork.Repository<AccountMapping>().AddAsync(mapping);
        }

        await _unitOfWork.SaveChangesAsync();
        */
        await Task.CompletedTask;
    }

    private async Task SeedDefaultFiscalYearAsync(Guid tenantId)
    {
        var currentYear = DateTime.UtcNow.Year;
        var fiscalYear = new FiscalYear
        {
            Name = $"FY-{currentYear}",
            Year = currentYear,
            StartDate = new DateTime(currentYear, 1, 1),
            EndDate = new DateTime(currentYear, 12, 31),
            Status = EnterpriseERP.Domain.Entities.Accounting.FiscalYearStatus.Open,
            TenantId = tenantId
        };

        await _unitOfWork.Repository<FiscalYear>().AddAsync(fiscalYear);
        await _unitOfWork.SaveChangesAsync();
    }
}
