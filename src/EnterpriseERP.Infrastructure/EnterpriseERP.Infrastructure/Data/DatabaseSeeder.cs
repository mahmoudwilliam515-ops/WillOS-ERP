using EnterpriseERP.Domain.Enums;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Settings;
using EnterpriseERP.Domain.Entities.SaaS;
using EnterpriseERP.Domain.Entities.HR;
using TreasuryEntities = EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.Domain.Entities.CRM;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Infrastructure.Identity;
using EnterpriseERP.Domain.Entities.Quality;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseERP.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    private readonly Guid _defaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }

            await SeedTenantsAsync();
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedBranchesAsync();
            await _context.SaveChangesAsync();
            
            var branchId = await _context.Branches.Where(b => b.TenantId == _defaultTenantId).Select(b => b.Id).FirstOrDefaultAsync();
            await SeedWarehousesAsync(branchId);
            await SeedItemsAsync();
            await SeedCustomersAsync();
            await SeedSuppliersAsync();
            await SeedAccountsAsync();
            await SeedFiscalYearsAsync();
            await SeedCostCentersAsync();
            await SeedCurrenciesAsync();
            await SeedHRAsync(branchId);
            await SeedTreasuryAsync(branchId);
            await SeedCRMAsync();
            await SeedManufacturingAsync();
            await SeedSampleTransactionsAsync();
            
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedTenantsAsync()
    {
        if (!await _context.Set<Tenant>().AnyAsync(t => t.Id == _defaultTenantId))
        {
            _context.Set<Tenant>().Add(new Tenant 
            { 
                Id = _defaultTenantId, 
                Name = "Default Test Tenant", 
                SubDomain = "test.erp.com",
                Status = TenantStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedRolesAsync()
    {
        if (!await _roleManager.Roles.AnyAsync())
        {
            var adminRole = new ApplicationRole { Name = "Admin", Description = "Administrator" };
            await _roleManager.CreateAsync(adminRole);
            await _roleManager.CreateAsync(new ApplicationRole { Name = "User", Description = "Standard User" });
            
            // Seed permissions for Admin role dynamically
            var role = await _roleManager.FindByNameAsync("Admin");
            if (role != null)
            {
                var permissionFields = typeof(EnterpriseERP.Domain.Entities.Identity.Permissions).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                var rolePermissions = new List<RolePermission>();
                
                foreach (var field in permissionFields)
                {
                    if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                    {
                        var permissionValue = (string)field.GetValue(null)!;
                        var module = permissionValue.Split('.')[0];
                        rolePermissions.Add(new RolePermission
                        {
                            Id = Guid.NewGuid(),
                            RoleId = role.Id,
                            Permission = permissionValue,
                            Module = module
                        });
                    }
                }

                if (rolePermissions.Any())
                {
                    await _context.Set<RolePermission>().AddRangeAsync(rolePermissions);
                    await _context.SaveChangesAsync();
                }
            }
        }
        else
        {
            // Even if roles exist, ensure Admin has all permissions in case new ones were added
            var role = await _roleManager.FindByNameAsync("Admin");
            if (role != null)
            {
                var existingPermissions = await _context.Set<RolePermission>().Where(p => p.RoleId == role.Id).Select(p => p.Permission).ToListAsync();
                var permissionFields = typeof(EnterpriseERP.Domain.Entities.Identity.Permissions).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                var newPermissions = new List<RolePermission>();
                
                foreach (var field in permissionFields)
                {
                    if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                    {
                        var permissionValue = (string)field.GetValue(null)!;
                        if (!existingPermissions.Contains(permissionValue))
                        {
                            var module = permissionValue.Split('.')[0];
                            newPermissions.Add(new RolePermission
                            {
                                Id = Guid.NewGuid(),
                                RoleId = role.Id,
                                Permission = permissionValue,
                                Module = module
                            });
                        }
                    }
                }

                if (newPermissions.Any())
                {
                    await _context.Set<RolePermission>().AddRangeAsync(newPermissions);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _userManager.FindByEmailAsync("admin@erp.com") == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@erp.com",
                Email = "admin@erp.com",
                FullName = "System Administrator",
                IsActive = true,
                TenantId = _defaultTenantId
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    private async Task SeedBranchesAsync()
    {
        if (!await _context.Branches.AnyAsync())
        {
            _context.Branches.AddRange(
                new Branch { Id = Guid.NewGuid(), Name = "Main Branch", Address = "123 ERP St.", Phone = "123456789", IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId },
                new Branch { Id = Guid.NewGuid(), Name = "Branch A", Address = "456 ERP Ave.", Phone = "987654321", IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId }
            );
        }
    }

    private async Task SeedWarehousesAsync(Guid branchId)
    {
        if (!await _context.Warehouses.AnyAsync() && branchId != Guid.Empty)
        {
            _context.Warehouses.AddRange(
                new Warehouse { Id = Guid.NewGuid(), Name = "Main Warehouse", BranchId = branchId, Address = "123 ERP St.", IsMain = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId },
                new Warehouse { Id = Guid.NewGuid(), Name = "Warehouse A", BranchId = branchId, Address = "456 ERP Ave.", IsMain = false, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId }
            );
        }
    }

    private async Task SeedItemsAsync()
    {
        if (!await _context.Items.AnyAsync())
        {
            _context.Items.AddRange(
                new Item { Id = Guid.NewGuid(), Code = "ITM001", Barcode = "1234567890123", NameAr = "صنف 1", NameEn = "Item 1", BuyPrice = 10, LastBuyPrice = 10, AverageCost = 10, MinStock = 5, MaxStock = 100, ReorderPoint = 10, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId },
                new Item { Id = Guid.NewGuid(), Code = "ITM002", Barcode = "1234567890124", NameAr = "صنف 2", NameEn = "Item 2", BuyPrice = 20, LastBuyPrice = 20, AverageCost = 20, MinStock = 5, MaxStock = 100, ReorderPoint = 10, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId }
            );
        }
    }

    private async Task SeedCustomersAsync()
    {
        if (!await _context.Customers.AnyAsync())
        {
            _context.Customers.AddRange(
                new Customer { Id = Guid.NewGuid(), Code = "CUS001", Name = "Customer 1", Phone = "111111111", Mobile = "222222222", Address = "Customer Address", CreditLimit = 1000, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId },
                new Customer { Id = Guid.NewGuid(), Code = "CUS002", Name = "Customer 2", Phone = "333333333", Mobile = "444444444", Address = "Customer 2 Address", CreditLimit = 2000, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId }
            );
        }
    }

    private async Task SeedSuppliersAsync()
    {
        if (!await _context.Suppliers.AnyAsync())
        {
            _context.Suppliers.AddRange(
                new Supplier { Id = Guid.NewGuid(), Code = "SUP001", Name = "Supplier 1", Phone = "555555555", Address = "Supplier Address", CreditLimit = 5000, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId },
                new Supplier { Id = Guid.NewGuid(), Code = "SUP002", Name = "Supplier 2", Phone = "666666666", Address = "Supplier 2 Address", CreditLimit = 10000, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId }
            );
        }
    }

    private async Task SeedAccountsAsync()
    {
        if (!await _context.Accounts.AnyAsync())
        {
            var assets = new Account { Id = Guid.NewGuid(), Code = "1", Name = "Assets (الأصول)", Type = AccountType.Asset, IsLeaf = false, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var liabilities = new Account { Id = Guid.NewGuid(), Code = "2", Name = "Liabilities (الخصوم)", Type = AccountType.Liability, IsLeaf = false, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var equity = new Account { Id = Guid.NewGuid(), Code = "3", Name = "Equity (حقوق الملكية)", Type = AccountType.Equity, IsLeaf = false, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var revenue = new Account { Id = Guid.NewGuid(), Code = "4", Name = "Revenue (الإيرادات)", Type = AccountType.Revenue, IsLeaf = false, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var expenses = new Account { Id = Guid.NewGuid(), Code = "5", Name = "Expenses (المصروفات)", Type = AccountType.Expense, IsLeaf = false, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };

            _context.Accounts.AddRange(assets, liabilities, equity, revenue, expenses);

            // Seed basic leaves
            var cashBox = new Account { Id = Guid.NewGuid(), Code = "111", Name = "Main Cash Box (الصندوق الرئيسي)", Type = AccountType.Asset, ParentId = assets.Id, IsLeaf = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var mainBank = new Account { Id = Guid.NewGuid(), Code = "112", Name = "Main Bank (البنك الرئيسي)", Type = AccountType.Asset, ParentId = assets.Id, IsLeaf = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var salesRev = new Account { Id = Guid.NewGuid(), Code = "411", Name = "Sales Revenue (إيرادات المبيعات)", Type = AccountType.Revenue, ParentId = revenue.Id, IsLeaf = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            
            _context.Accounts.AddRange(cashBox, mainBank, salesRev);

            var ar = new Account { Id = Guid.NewGuid(), Code = "1200", Name = "Accounts Receivable", Type = AccountType.Asset, ParentId = assets.Id, IsLeaf = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var ap = new Account { Id = Guid.NewGuid(), Code = "2100", Name = "Accounts Payable", Type = AccountType.Liability, ParentId = liabilities.Id, IsLeaf = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var inventory = new Account { Id = Guid.NewGuid(), Code = "1300", Name = "Inventory", Type = AccountType.Asset, ParentId = assets.Id, IsLeaf = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var cogs = new Account { Id = Guid.NewGuid(), Code = "5100", Name = "COGS", Type = AccountType.Expense, ParentId = expenses.Id, IsLeaf = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var salesReturn = new Account { Id = Guid.NewGuid(), Code = "4120", Name = "Sales Returns", Type = AccountType.Revenue, ParentId = revenue.Id, IsLeaf = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var purchaseReturn = new Account { Id = Guid.NewGuid(), Code = "5120", Name = "Purchase Returns", Type = AccountType.Expense, ParentId = expenses.Id, IsLeaf = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var purchases = new Account { Id = Guid.NewGuid(), Code = "5110", Name = "Purchases", Type = AccountType.Expense, ParentId = expenses.Id, IsLeaf = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var taxPayable = new Account { Id = Guid.NewGuid(), Code = "2200", Name = "VAT Payable", Type = AccountType.Liability, ParentId = liabilities.Id, IsLeaf = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };
            var taxReceivable = new Account { Id = Guid.NewGuid(), Code = "1150", Name = "VAT Receivable", Type = AccountType.Asset, ParentId = assets.Id, IsLeaf = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId };

            _context.Accounts.AddRange(ar, ap, inventory, cogs, salesReturn, purchaseReturn, purchases, taxPayable, taxReceivable);

            var companyId = Guid.Empty; // Default company for mapping
            await _context.AccountMappings.AddRangeAsync(
                new AccountMapping { PostingKey = PostingKey.CASH_ACCOUNT, AccountId = cashBox.Id, TenantId = _defaultTenantId, CompanyId = companyId },
                new AccountMapping { PostingKey = PostingKey.AR_RECEIVABLE, AccountId = ar.Id, TenantId = _defaultTenantId, CompanyId = companyId },
                new AccountMapping { PostingKey = PostingKey.AP_PAYABLE, AccountId = ap.Id, TenantId = _defaultTenantId, CompanyId = companyId },
                new AccountMapping { PostingKey = PostingKey.SALES_REVENUE, AccountId = salesRev.Id, TenantId = _defaultTenantId, CompanyId = companyId },
                new AccountMapping { PostingKey = PostingKey.SALES_DISCOUNT, AccountId = salesReturn.Id, TenantId = _defaultTenantId, CompanyId = companyId }, // Reused for simplicity
                new AccountMapping { PostingKey = PostingKey.PURCHASE_EXPENSE, AccountId = purchases.Id, TenantId = _defaultTenantId, CompanyId = companyId },
                new AccountMapping { PostingKey = PostingKey.INVENTORY_ASSET, AccountId = inventory.Id, TenantId = _defaultTenantId, CompanyId = companyId },
                new AccountMapping { PostingKey = PostingKey.COGS, AccountId = cogs.Id, TenantId = _defaultTenantId, CompanyId = companyId },
                new AccountMapping { PostingKey = PostingKey.SALES_TAX_PAYABLE, AccountId = taxPayable.Id, TenantId = _defaultTenantId, CompanyId = companyId },
                new AccountMapping { PostingKey = PostingKey.VAT_INPUT, AccountId = taxReceivable.Id, TenantId = _defaultTenantId, CompanyId = companyId }
            );
        }
        else if (!await _context.AccountMappings.AnyAsync())
        {
            await SeedAccountMappingsFromExistingAccountsAsync();
        }
    }

    private async Task SeedAccountMappingsFromExistingAccountsAsync()
    {
        var accounts = await _context.Accounts.Where(a => a.IsLeaf).ToListAsync();
        Account? FindByCode(string code) => accounts.FirstOrDefault(a => a.Code == code);

        void AddMap(PostingKey key, string code)
        {
            var acc = FindByCode(code);
            if (acc != null)
            {
                _context.AccountMappings.Add(new AccountMapping
                {
                    PostingKey = key,
                    AccountId = acc.Id,
                    TenantId = _defaultTenantId,
                    CompanyId = Guid.Empty // Or set a default
                });
            }
        }

        AddMap(PostingKey.CASH_ACCOUNT, "111");
        AddMap(PostingKey.AR_RECEIVABLE, "1200");
        AddMap(PostingKey.AP_PAYABLE, "2100");
        AddMap(PostingKey.SALES_REVENUE, "411");
        AddMap(PostingKey.INVENTORY_ASSET, "1300");
        AddMap(PostingKey.COGS, "5100");
        AddMap(PostingKey.SALES_TAX_PAYABLE, "2200");
        AddMap(PostingKey.VAT_INPUT, "1150");
        await _context.SaveChangesAsync();
    }

    private async Task SeedFiscalYearsAsync()
    {
        if (!await _context.FiscalYears.AnyAsync())
        {
            var year = new FiscalYear
            {
                Id = Guid.NewGuid(),
                Name = DateTime.UtcNow.Year.ToString(),
                Year = DateTime.UtcNow.Year,
                StartDate = new DateTime(DateTime.UtcNow.Year, 1, 1),
                EndDate = new DateTime(DateTime.UtcNow.Year, 12, 31),
                Status = FiscalYearStatus.Open,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow,
                TenantId = _defaultTenantId
            };
            _context.FiscalYears.Add(year);

            // Create default periods (12 months)
            for (int i = 1; i <= 12; i++)
            {
                var period = new AccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    FiscalYearId = year.Id,
                    PeriodName = $"Period {i}",
                    StartDate = new DateTime(DateTime.UtcNow.Year, i, 1),
                    EndDate = new DateTime(DateTime.UtcNow.Year, i, DateTime.DaysInMonth(DateTime.UtcNow.Year, i)),
                    Status = AccountingPeriodStatus.Open,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow,
                    TenantId = _defaultTenantId
                };
                _context.AccountingPeriods.Add(period);
            }
        }
    }

    private async Task SeedCostCentersAsync()
    {
        if (!await _context.CostCenters.AnyAsync())
        {
            _context.CostCenters.AddRange(
                new CostCenter { Id = Guid.NewGuid(), Code = "CC001", Name = "Main Operations", IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId },
                new CostCenter { Id = Guid.NewGuid(), Code = "CC002", Name = "Administration", IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId }
            );
        }
    }

    private async Task SeedCurrenciesAsync()
    {
        if (!await _context.Currencies.AnyAsync())
        {
            _context.Currencies.AddRange(
                new Currency { Id = Guid.NewGuid(), Code = "EGP", Name = "Egyptian Pound", Symbol = "ج.م", IsBaseCurrency = true, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId },
                new Currency { Id = Guid.NewGuid(), Code = "USD", Name = "US Dollar", Symbol = "$", IsBaseCurrency = false, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId },
                new Currency { Id = Guid.NewGuid(), Code = "EUR", Name = "Euro", Symbol = "€", IsBaseCurrency = false, IsActive = true, CreatedBy = "System", CreatedAt = DateTime.UtcNow, TenantId = _defaultTenantId }
            );
        }
    }

    private async Task SeedHRAsync(Guid branchId)
    {
        if (!await _context.Employees.AnyAsync() && branchId != Guid.Empty)
        {
            _context.Employees.AddRange(
                new Employee 
                { 
                    Id = Guid.NewGuid(), 
                    EmployeeNumber = "EMP001", 
                    FirstName = "Ahmed", 
                    LastName = "Mohamed", 
                    JobTitle = "Manager", 
                    Department = "Operations", 
                    BasicSalary = 10000, 
                    HireDate = DateTime.UtcNow.AddYears(-1), 
                    BranchId = branchId, 
                    IsActive = true, 
                    CreatedBy = "System", 
                    CreatedAt = DateTime.UtcNow, 
                    TenantId = _defaultTenantId 
                },
                new Employee 
                { 
                    Id = Guid.NewGuid(), 
                    EmployeeNumber = "EMP002", 
                    FirstName = "Sara", 
                    LastName = "Ahmed", 
                    JobTitle = "Accountant", 
                    Department = "Finance", 
                    BasicSalary = 7000, 
                    HireDate = DateTime.UtcNow.AddMonths(-6), 
                    BranchId = branchId, 
                    IsActive = true, 
                    CreatedBy = "System", 
                    CreatedAt = DateTime.UtcNow, 
                    TenantId = _defaultTenantId 
                }
            );
        }
    }

    private async Task SeedTreasuryAsync(Guid branchId)
    {
        if (branchId == Guid.Empty) return;

        var cashAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == "111");
        var bankAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == "112");

        if (!await _context.CashAccounts.AnyAsync() && cashAccount != null)
        {
            _context.CashAccounts.Add(new TreasuryEntities.CashAccount 
            { 
                Id = Guid.NewGuid(), 
                Code = "CSH01", 
                Name = "Main Safe", 
                BranchId = branchId, 
                LinkedAccountId = cashAccount.Id, 
                Currency = "EGP", 
                CreatedBy = "System", 
                CreatedAt = DateTime.UtcNow, 
                TenantId = _defaultTenantId 
            });
        }

        if (!await _context.BankAccounts.AnyAsync() && bankAccount != null)
        {
            _context.BankAccounts.Add(new TreasuryEntities.BankAccount 
            { 
                Id = Guid.NewGuid(), 
                Code = "BNK01", 
                BankName = "CIB", 
                AccountNumber = "1234567890", 
                IBAN = "EG0001234567890", 
                LinkedAccountId = bankAccount.Id, 
                Currency = "EGP", 
                CreatedBy = "System", 
                CreatedAt = DateTime.UtcNow, 
                TenantId = _defaultTenantId 
            });
        }
    }

    private async Task SeedCRMAsync()
    {
        if (!await _context.Leads.AnyAsync())
        {
            _context.Leads.AddRange(
                new Lead 
                { 
                    Id = Guid.NewGuid(), 
                    FirstName = "John", 
                    LastName = "Doe", 
                    CompanyName = "Nexus Dynamics", 
                    Email = "j.doe@nexus.com", 
                    Phone = "+201012345678", 
                    Status = LeadStatus.New, 
                    EstimatedValue = 50000, 
                    CreatedBy = "System", 
                    CreatedAt = DateTime.UtcNow, 
                    TenantId = _defaultTenantId 
                },
                new Lead 
                { 
                    Id = Guid.NewGuid(), 
                    FirstName = "Jane", 
                    LastName = "Smith", 
                    CompanyName = "Vertex Solutions", 
                    Email = "jane@vertex.io", 
                    Phone = "+201112223334", 
                    Status = LeadStatus.Qualified, 
                    EstimatedValue = 150000, 
                    CreatedBy = "System", 
                    CreatedAt = DateTime.UtcNow, 
                    TenantId = _defaultTenantId 
                }
            );
        }
    }

    private async Task SeedManufacturingAsync()
    {
        if (!await _context.WorkCenters.AnyAsync())
        {
            _context.WorkCenters.AddRange(
                new WorkCenter { Id = Guid.NewGuid(), Name = "Assembly Line A", Code = "WC-ASSY-A", HourlyRate = 50, OverheadRate = 30, TenantId = _defaultTenantId },
                new WorkCenter { Id = Guid.NewGuid(), Name = "Testing Station 1", Code = "WC-TEST-1", HourlyRate = 40, OverheadRate = 20, TenantId = _defaultTenantId }
            );
            await _context.SaveChangesAsync();
        }

        if (!await _context.BillOfMaterials.AnyAsync())
        {
            var product = await _context.Items.FirstOrDefaultAsync(i => i.Code == "ITM001");
            var material = await _context.Items.FirstOrDefaultAsync(i => i.Code == "ITM002");
            var workCenter = await _context.WorkCenters.FirstOrDefaultAsync(w => w.Code == "WC-ASSY-A");

            if (product != null && material != null)
            {
                var bom = new BillOfMaterials
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Name = "Standard Laptop Assembly",
                    Description = "Main assembly BOM for Enterprise Laptop",
                    Version = "1.0",
                    IsDefault = true,
                    Status = BOMStatus.Active,
                    LaborCostPerHour = 50,
                    MachineOverheadPerHour = 30,
                    TotalEstimatedCost = material.BuyPrice + 80,
                    TenantId = _defaultTenantId,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                };

                bom.Lines.Add(new BillOfMaterialsLine
                {
                    Id = Guid.NewGuid(),
                    RawMaterialId = material.Id,
                    Quantity = 1,
                    Unit = "pcs",
                    ScrapFactor = 0
                });

                bom.Stages.Add(new ProductionStage
                {
                    Id = Guid.NewGuid(),
                    Name = "Assembly",
                    Sequence = 1,
                    EstimatedHours = 1,
                    CostPerHour = 50,
                    WorkCenterId = workCenter?.Id,
                    Description = "Primary assembly of components"
                });

                _context.BillOfMaterials.Add(bom);
            }
        }
    }

    private async Task SeedSampleTransactionsAsync()
    {
        if (!await _context.JournalEntries.AnyAsync())
        {
            var cashAcc = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == "111");
            var capitalAcc = await _context.Accounts.FirstOrDefaultAsync(a => a.Code == "3"); // Equity root for demo or add a leaf

            if (cashAcc != null && capitalAcc != null)
            {
                var entry = new JournalEntry
                {
                    Id = Guid.NewGuid(),
                    EntryNumber = "JE-INIT-001",
                    EntryDate = DateTime.UtcNow.AddDays(-30),
                    Description = "Initial Capital Injection",
                    Status = JournalEntryStatus.Posted,
                    CurrencyCode = "EGP",
                    ExchangeRate = 1.0m,
                    TotalDebit = 1000000,
                    TotalCredit = 1000000,
                    TenantId = _defaultTenantId,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow
                };

                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = cashAcc.Id,
                    AccountCode = cashAcc.Code,
                    AccountName = cashAcc.Name,
                    DebitAmount = 1000000,
                    BaseDebitAmount = 1000000,
                    Description = "Cash Investment"
                });

                entry.Lines.Add(new JournalEntryLine
                {
                    AccountId = capitalAcc.Id,
                    AccountCode = capitalAcc.Code,
                    AccountName = capitalAcc.Name,
                    CreditAmount = 1000000,
                    BaseCreditAmount = 1000000,
                    Description = "Owner Capital"
                });

                _context.JournalEntries.Add(entry);
            }
        }

        if (!await _context.QualityChecklists.AnyAsync())
        {
            var checklist = new QualityChecklist
            {
                Id = Guid.NewGuid(),
                Name = "إجراء استلام المواد الخام",
                Description = "قائمة التحقق القياسية لاستلام المواد الخام من الموردين",
                Type = InspectionType.PurchaseReceipt,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = _defaultTenantId,
                Items = new List<QualityChecklistItem>
                {
                    new() { Id = Guid.NewGuid(), Requirement = "سلامة التغليف الخارجي", IsMandatory = true, Sequence = 1 },
                    new() { Id = Guid.NewGuid(), Requirement = "تطابق الكمية مع بوليصة الشحن", IsMandatory = true, Sequence = 2 },
                    new() { Id = Guid.NewGuid(), Requirement = "تاريخ الصلاحية (إن وجد)", IsMandatory = true, Sequence = 3 }
                }
            };

            var prodChecklist = new QualityChecklist
            {
                Id = Guid.NewGuid(),
                Name = "فحص المنتج النهائي",
                Description = "قائمة التحقق قبل استلام المنتج التام في المخازن",
                Type = InspectionType.ProductionOutput,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                TenantId = _defaultTenantId,
                Items = new List<QualityChecklistItem>
                {
                    new() { Id = Guid.NewGuid(), Requirement = "المظهر العام والتشطيب", IsMandatory = true, Sequence = 1 },
                    new() { Id = Guid.NewGuid(), Requirement = "القياسات والأبعاد القياسية", IsMandatory = true, Sequence = 2 },
                    new() { Id = Guid.NewGuid(), Requirement = "اختبار الوظيفة الأساسية", IsMandatory = true, Sequence = 3 }
                }
            };

            _context.QualityChecklists.AddRange(checklist, prodChecklist);
        }
    }
}
