using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Infrastructure.Identity;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Settings;
using EnterpriseERP.Domain.Entities.FixedAssets;
using EnterpriseERP.Domain.Entities.HR;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.Domain.Entities.SaaS;
using EnterpriseERP.Domain.Entities.CRM;
using EnterpriseERP.Domain.Entities.Audit;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Domain.Entities.Workflow;
using EnterpriseERP.Domain.Entities.Projects;
using EnterpriseERP.Domain.Entities.Quality;
using EnterpriseERP.Domain.Entities.Taxation;
using EnterpriseERP.Domain.Entities.EInvoice;
using EnterpriseERP.Domain.Entities.Reporting;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.Domain.Treasury;
using EnterpriseERP.Domain.Reporting;
using EnterpriseERP.Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Exceptions;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using AccountingEntities = EnterpriseERP.Domain.Entities.Accounting;
using TreasuryEntities = EnterpriseERP.Domain.Entities.Treasury;
using FixedAssetsEntities = EnterpriseERP.Domain.Entities.FixedAssets;

namespace EnterpriseERP.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string,
        IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>, IAppDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantConnectionProvider? _connectionProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        ITenantConnectionProvider? connectionProvider = null)
        : base(options)
    {
        _currentUserService = currentUserService;
        _connectionProvider = connectionProvider;
    }

    public Guid CurrentTenantId => _currentUserService?.TenantId ?? Guid.Empty;
    public Guid CurrentCompanyId => _currentUserService?.CompanyId ?? Guid.Empty;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_connectionProvider != null && !optionsBuilder.IsConfigured)
        {
            var connectionString = _connectionProvider.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseSqlServer(connectionString, options =>
                {
                    // Enable query splitting to avoid Cartesian Explosion in complex queries
                    options.EnableRetryOnFailure(maxRetryCount: 5);
                    options.CommandTimeout(30);
                });
            }
        }

        // Query Splitting Behavior - Split queries to avoid Cartesian Explosion

        // Query Tracking Behavior - Use NoTracking for read-only queries by default
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        // Enable sensitive data logging and detailed errors for development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        base.OnConfiguring(optionsBuilder);
    }

    // Identity Extended
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    // ERP Core
    public DbSet<Branch> Branches { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
    public DbSet<Warehouse> Warehouses { get; set; } = null!;
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; } = null!;
    public DbSet<ItemBatch> ItemBatches { get; set; } = null!;
    public DbSet<ItemSerial> ItemSerials { get; set; } = null!;
    public DbSet<InventoryTransfer> InventoryTransfers { get; set; } = null!;
    public DbSet<InventoryAdjustment> InventoryAdjustments { get; set; } = null!;
    public DbSet<InventoryReservation> InventoryReservations { get; set; } = null!;
    
    // Procurement (Sprint 2)
    public DbSet<EnterpriseERP.Domain.Procurement.GoodsReceiptNote> GoodsReceiptNotes { get; set; } = null!;
    public DbSet<GoodsReceiptNoteLine> GoodsReceiptNoteLines { get; set; } = null!;
    public DbSet<MatchToleranceRule> MatchToleranceRules { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Procurement.PurchaseReturn> PurchaseReturns { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Procurement.DebitNote> DebitNotes { get; set; } = null!;

    public DbSet<DeliveryNote> DeliveryNotes { get; set; } = null!;
    public DbSet<DeliveryNoteLine> DeliveryNoteLines { get; set; } = null!;
    public DbSet<InventoryBalance> InventoryBalances { get; set; } = null!;
    public DbSet<InventoryFIFOLayer> InventoryFIFOLayers { get; set; } = null!;
    public DbSet<CycleCount> CycleCounts { get; set; } = null!;
    public DbSet<CycleCountLine> CycleCountLines { get; set; } = null!;
    public DbSet<SalesInvoice> SalesInvoices { get; set; } = null!;
    public DbSet<SalesInvoiceLine> SalesInvoiceLines { get; set; } = null!;
    
    // Sales (Sprint 2)
    public DbSet<EnterpriseERP.Domain.Sales.SalesOrder> SalesOrders { get; set; } = null!;
    public DbSet<SalesOrderLine> SalesOrderLines { get; set; } = null!;
    public DbSet<ReservationEntry> ReservationEntries { get; set; } = null!;
    public DbSet<DunningNotice> DunningNotices { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Sales.SalesReturn> SalesReturns { get; set; } = null!;
    public DbSet<SalesReturnLine> SalesReturnLines { get; set; } = null!;

    public DbSet<SalesQuotation> SalesQuotations { get; set; } = null!;
    public DbSet<CreditNote> CreditNotes { get; set; } = null!;
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; } = null!;
    public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; } = null!;
    public DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; } = null!;
    public DbSet<PurchaseRequisitionLine> PurchaseRequisitionLines { get; set; } = null!;
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; } = null!;
    public DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines { get; set; } = null!;
    
    // Treasury (Sprint 2)
    public DbSet<EnterpriseERP.Domain.Treasury.PaymentProposal> PaymentProposals { get; set; } = null!;
    public DbSet<PaymentProposalLine> PaymentProposalLines { get; set; } = null!;
    public DbSet<EnterpriseERP.Application.Services.BankStatement.UnmatchedBankTransaction> UnmatchedBankTransactions { get; set; } = null!;

    public DbSet<PurchaseReturnLine> PurchaseReturnLines { get; set; } = null!;
    public DbSet<WarehouseBin> WarehouseBins { get; set; } = null!;
    public DbSet<TransferOrder> TransferOrders { get; set; } = null!;
    public DbSet<TransferOrderLine> TransferOrderLines { get; set; } = null!;
    public DbSet<QualityChecklist> QualityChecklists { get; set; } = null!;
    public DbSet<QualityChecklistItem> QualityChecklistItems { get; set; } = null!;
    public DbSet<QualityInspection> QualityInspections { get; set; } = null!;
    public DbSet<InspectionResultItem> InspectionResultItems { get; set; } = null!;
    
    // HR & Payroll
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<JobPosition> JobPositions { get; set; } = null!;
    public DbSet<EmployeeGrade> EmployeeGrades { get; set; } = null!;
    public DbSet<EmployeeContract> EmployeeContracts { get; set; } = null!;
    public DbSet<PayrollRun> PayrollRuns { get; set; } = null!;
    public DbSet<PayrollEntry> PayrollEntries { get; set; } = null!;

    // Financial Excellence
    public DbSet<AccountingEntities.FixedAsset> AccountingFixedAssets { get; set; } = null!;
    public DbSet<AssetCategory> AssetCategories { get; set; } = null!;
    public DbSet<AssetDepreciationLog> AssetDepreciationLogs { get; set; } = null!;
    public DbSet<AccountingEntities.BankAccount> AccountingBankAccounts { get; set; } = null!;
    public DbSet<AccountingEntities.BankReconciliation> AccountingBankReconciliations { get; set; } = null!;

    public DbSet<ToleranceRule> ToleranceRules { get; set; } = null!;
    public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
    public DbSet<JournalEntryLine> JournalEntryLines { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<CostCenter> CostCenters { get; set; } = null!;
    public DbSet<FiscalYear> FiscalYears { get; set; } = null!;
    public DbSet<AccountingPeriod> AccountingPeriods { get; set; } = null!;
    public DbSet<Budget> Budgets { get; set; } = null!;
    public DbSet<BudgetLine> BudgetLines { get; set; } = null!;
    public DbSet<AccountMapping> AccountMappings { get; set; } = null!;
    public DbSet<IntercompanyTransaction> IntercompanyTransactions { get; set; } = null!;
    public DbSet<ConsolidationRun> ConsolidationRuns { get; set; } = null!;
    public DbSet<Currency> Currencies { get; set; } = null!;
    public DbSet<ExchangeRate> ExchangeRates { get; set; } = null!;
    
    // Treasury
    public DbSet<CashAccount> CashAccounts { get; set; } = null!;
    public DbSet<TreasuryEntities.BankAccount> BankAccounts { get; set; } = null!;
    public DbSet<ReceiptVoucher> ReceiptVouchers { get; set; } = null!;
    public DbSet<PaymentVoucher> PaymentVouchers { get; set; } = null!;
    public DbSet<BankTransfer> BankTransfers { get; set; } = null!;
    public DbSet<TreasuryEntities.BankReconciliation> BankReconciliations { get; set; } = null!;
    public DbSet<TreasuryEntities.BankReconciliationLine> BankReconciliationLines { get; set; } = null!;
    public DbSet<InvoicePayment> InvoicePayments { get; set; } = null!;

    // SaaS & CRM
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
    public DbSet<TenantInvoice> TenantInvoices { get; set; } = null!;
    public DbSet<Lead> Leads { get; set; } = null!;
    public DbSet<Opportunity> Opportunities { get; set; } = null!;

    // Fixed Assets
    public DbSet<FixedAssetsEntities.FixedAsset> FixedAssets { get; set; } = null!;
    public DbSet<DepreciationTransaction> DepreciationTransactions { get; set; } = null!;

    // HR
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<EmployeeDeduction> EmployeeDeductions { get; set; } = null!;
    public DbSet<PayrollTransaction> PayrollTransactions { get; set; } = null!;

    // Audit
    public DbSet<EnterpriseERP.Domain.Entities.Audit.AuditLog> AuditLogs { get; set; } = null!;

    // Manufacturing
    public DbSet<EnterpriseERP.Domain.Entities.Manufacturing.RawMaterial> RawMaterials { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Manufacturing.BillOfMaterials> BillOfMaterials { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Manufacturing.WorkCenter> WorkCenters { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Manufacturing.Routing> Routings { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Manufacturing.RoutingStep> RoutingSteps { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Manufacturing.BillOfMaterialsLine> BillOfMaterialsLines { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Manufacturing.ProductionStage> ProductionStages { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Manufacturing.WorkCenterCalendar> WorkCenterCalendars { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Manufacturing.WorkCenterException> WorkCenterExceptions { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Manufacturing.ProductionOrder> ProductionOrders { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Manufacturing.ProductionOrderMaterial> ProductionOrderMaterials { get; set; } = null!;

    // Maintenance
    public DbSet<EnterpriseERP.Domain.Entities.Maintenance.MaintenanceTechnician> MaintenanceTechnicians { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Maintenance.MaintenanceWorkOrder> MaintenanceWorkOrders { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Maintenance.MaintenanceWorkOrderPart> MaintenanceWorkOrderParts { get; set; } = null!;

    // Migration & Opening Balances
    public DbSet<EnterpriseERP.Domain.Entities.Migration.OpeningBalanceSession> OpeningBalanceSessions { get; set; } = null!;

    // Party Balances (Snapshot)
    public DbSet<EnterpriseERP.Domain.Entities.Accounting.PartyBalance> PartyBalances { get; set; } = null!;

    // Workflow / Approvals
    public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; } = null!;
    public DbSet<ApprovalRequest> ApprovalRequests { get; set; } = null!;
    public DbSet<ApprovalAction> ApprovalActions { get; set; } = null!;
    
    // Enterprise Workflow Engine (Section 10)
    public DbSet<WorkflowInstance> WorkflowInstances { get; set; } = null!;
    public DbSet<WorkflowTask> WorkflowTasks { get; set; } = null!;
    public DbSet<WorkflowAuditLog> WorkflowAuditLogs => Set<WorkflowAuditLog>();
    public DbSet<ApprovalRule> ApprovalRules => Set<ApprovalRule>();
    public DbSet<DelegationRule> DelegationRules => Set<DelegationRule>();
    public DbSet<AuditApiLog> AuditApiLogs => Set<AuditApiLog>();

    // Projects / WBS / Timesheets
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectTask> ProjectTasks { get; set; } = null!;
    public DbSet<Timesheet> Timesheets { get; set; } = null!;
    public DbSet<TimesheetLine> TimesheetLines { get; set; } = null!;

    // Taxation
    public DbSet<TaxCategory> TaxCategories { get; set; } = null!;
    public DbSet<TaxRule> TaxRules { get; set; } = null!;

    // E-Invoice
    public DbSet<EInvoiceDocument> EInvoiceDocuments { get; set; } = null!;

    // Reporting & BI
    public DbSet<ReportTemplate> ReportTemplates { get; set; } = null!;
    public DbSet<ReportSection> ReportSections { get; set; } = null!;
    public DbSet<ReportLine> ReportLines { get; set; } = null!;

    // Period Close (Sprint 2)
    public DbSet<EnterpriseERP.Domain.Reporting.PeriodCloseChecklist> PeriodCloseChecklists { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Reporting.PeriodCloseChecklistStep> PeriodCloseChecklistItems { get; set; } = null!;
    public DbSet<EnterpriseERP.Domain.Entities.Accounting.GeneralLedgerEntry> GeneralLedgerEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Identity UserRole join table
        modelBuilder.Entity<ApplicationUserRole>(b =>
        {
            b.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
            b.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
        });

        // RolePermission
        modelBuilder.Entity<RolePermission>(b =>
        {
            b.HasOne(rp => rp.Role).WithMany(r => r.Permissions).HasForeignKey(rp => rp.RoleId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(rp => new { rp.RoleId, rp.Permission }).IsUnique();
        });

        modelBuilder.Entity<Company>(b =>
        {
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.Property(x => x.Code).HasMaxLength(20);
            b.Property(x => x.Name).HasMaxLength(200);
            b.Property(x => x.CountryCode).HasMaxLength(3);
            b.Property(x => x.FunctionalCurrencyCode).HasMaxLength(3);
            b.Property(x => x.TaxRegistrationNumber).HasMaxLength(100);
            b.HasOne(x => x.ParentCompany)
                .WithMany(x => x.Subsidiaries)
                .HasForeignKey(x => x.ParentCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IntercompanyTransaction>(b =>
        {
            b.HasIndex(x => new { x.TenantId, x.TransactionNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.SourceCompanyId, x.TargetCompanyId, x.TransactionDate });
            b.Property(x => x.TransactionNumber).HasMaxLength(40);
            b.Property(x => x.CurrencyCode).HasMaxLength(3);
            b.Property(x => x.Amount).HasPrecision(18, 4);
            b.HasOne(x => x.SourceCompany)
                .WithMany()
                .HasForeignKey(x => x.SourceCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.TargetCompany)
                .WithMany()
                .HasForeignKey(x => x.TargetCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ConsolidationRun>(b =>
        {
            b.HasIndex(x => new { x.TenantId, x.RunNumber }).IsUnique();
            b.Property(x => x.RunNumber).HasMaxLength(40);
            b.Property(x => x.IntercompanyAmount).HasPrecision(18, 4);
            b.Property(x => x.EliminatedAmount).HasPrecision(18, 4);
            b.Property(x => x.UnmatchedAmount).HasPrecision(18, 4);
            b.HasOne(x => x.GroupCompany)
                .WithMany()
                .HasForeignKey(x => x.GroupCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkflowDefinition>(b =>
        {
            b.HasIndex(x => new { x.TenantId, x.DocumentType, x.IsActive });
            b.Property(x => x.Name).HasMaxLength(150);
            b.Property(x => x.ApproverRole).HasMaxLength(100);
            b.Property(x => x.MinAmount).HasPrecision(18, 4);
            b.Property(x => x.MaxAmount).HasPrecision(18, 4);
        });

        modelBuilder.Entity<ApprovalRequest>(b =>
        {
            b.HasIndex(x => new { x.TenantId, x.DocumentType, x.DocumentId, x.Status });
            b.Property(x => x.DocumentNumber).HasMaxLength(80);
            b.Property(x => x.Amount).HasPrecision(18, 4);
            b.Property(x => x.RequestedBy).HasMaxLength(100);
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ApprovalAction>(b =>
        {
            b.HasIndex(x => new { x.TenantId, x.ApprovalRequestId, x.ActorUserId }).IsUnique();
            b.Property(x => x.ActorUserId).HasMaxLength(100);
            b.Property(x => x.Comment).HasMaxLength(500);
            b.HasOne(x => x.ApprovalRequest)
                .WithMany(x => x.Actions)
                .HasForeignKey(x => x.ApprovalRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Project>(b =>
        {
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.Property(x => x.Code).HasMaxLength(40);
            b.Property(x => x.Name).HasMaxLength(200);
            b.Property(x => x.CustomerName).HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.BudgetAmount).HasPrecision(18, 4);
            b.Property(x => x.ActualLaborCost).HasPrecision(18, 4);
            b.HasOne(x => x.CostCenter)
                .WithMany()
                .HasForeignKey(x => x.CostCenterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectTask>(b =>
        {
            b.HasIndex(x => new { x.TenantId, x.ProjectId, x.WbsCode }).IsUnique();
            b.Property(x => x.WbsCode).HasMaxLength(60);
            b.Property(x => x.Name).HasMaxLength(200);
            b.Property(x => x.PlannedHours).HasPrecision(18, 4);
            b.Property(x => x.ActualHours).HasPrecision(18, 4);
            b.Property(x => x.ActualLaborCost).HasPrecision(18, 4);
            b.HasOne(x => x.Project)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ParentTask)
                .WithMany(x => x.SubTasks)
                .HasForeignKey(x => x.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Timesheet>(b =>
        {
            b.HasIndex(x => new { x.TenantId, x.TimesheetNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.EmployeeId, x.PeriodStart, x.PeriodEnd });
            b.Property(x => x.TimesheetNumber).HasMaxLength(40);
            b.Property(x => x.EmployeeId).HasMaxLength(100);
            b.Property(x => x.SubmittedBy).HasMaxLength(100);
            b.Property(x => x.ApprovedBy).HasMaxLength(100);
            b.Property(x => x.RejectionReason).HasMaxLength(500);
            b.Property(x => x.TotalHours).HasPrecision(18, 4);
            b.Property(x => x.TotalLaborCost).HasPrecision(18, 4);
        });

        modelBuilder.Entity<TimesheetLine>(b =>
        {
            b.HasIndex(x => new { x.TenantId, x.ProjectId, x.ProjectTaskId, x.WorkDate });
            b.Property(x => x.Hours).HasPrecision(18, 4);
            b.Property(x => x.HourlyRate).HasPrecision(18, 4);
            b.Property(x => x.Description).HasMaxLength(500);
            b.HasOne(x => x.Timesheet)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.TimesheetId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ProjectTask)
                .WithMany()
                .HasForeignKey(x => x.ProjectTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Global Query Filters (Multi-Tenancy & Soft Delete)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            Expression? filterBody = null;

            // 1. Multi-Tenancy Filter - RULE-DB01: ÙƒÙ„ Ø¬Ø¯ÙˆÙ„ ØªØ´ØºÙŠÙ„ÙŠ ÙŠØ¬Ø¨ Ø£Ù† ÙŠØ­ØªÙˆÙŠ TenantId
            if (typeof(EnterpriseERP.SharedKernel.Common.ITenantEntity).IsAssignableFrom(entityType.ClrType) && entityType.ClrType != typeof(Tenant))
            {
                // Ensure isolation even if CurrentTenantId is empty
                var tenantFilter = Expression.AndAlso(
                    Expression.NotEqual(Expression.Property(Expression.Constant(this), nameof(CurrentTenantId)), Expression.Constant(Guid.Empty)),
                    Expression.Equal(
                        Expression.Property(parameter, nameof(EnterpriseERP.SharedKernel.Common.ITenantEntity.TenantId)),
                        Expression.Property(Expression.Constant(this), nameof(CurrentTenantId))
                    )
                );
                filterBody = tenantFilter;
            }

            // 1.1 Multi-Company Filter (Within Tenant)
            if (typeof(ICompanyEntity).IsAssignableFrom(entityType.ClrType) && entityType.ClrType != typeof(Company))
            {
                var companyFilter = Expression.AndAlso(
                    Expression.NotEqual(Expression.Property(Expression.Constant(this), nameof(CurrentCompanyId)), Expression.Constant(Guid.Empty)),
                    Expression.Equal(
                        Expression.Property(parameter, nameof(ICompanyEntity.CompanyId)),
                        Expression.Property(Expression.Constant(this), nameof(CurrentCompanyId))
                    )
                );
                filterBody = filterBody == null ? companyFilter : Expression.AndAlso(filterBody, companyFilter);
            }

            // 2. Soft Delete Filter - RULE-AUDIT01: AuditLog Ù„Ø§ ÙŠÙ…ÙƒÙ† Ø­Ø°ÙÙ‡ Ø£Ùˆ ØªØ¹Ø¯ÙŠÙ„Ù‡ (Append-Only)
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType) && entityType.ClrType != typeof(EnterpriseERP.Domain.Entities.Audit.AuditLog))
            {
                var softDeleteFilter = Expression.Not(Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)));
                filterBody = filterBody == null 
                    ? softDeleteFilter 
                    : Expression.AndAlso(filterBody, softDeleteFilter);
            }

            if (filterBody != null)
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(filterBody, parameter));
            }
        }

        // Decimal precision (18,4) for all financial fields
        modelBuilder.Entity<SalesInvoice>(e =>
        {
            e.Property(p => p.SubTotal).HasPrecision(18, 4);
            e.Property(p => p.DiscountAmount).HasPrecision(18, 4);
            e.Property(p => p.TaxAmount).HasPrecision(18, 4);
            e.Property(p => p.TotalAmount).HasPrecision(18, 4);
            e.Property(p => p.PaidAmount).HasPrecision(18, 4);
            e.Property(p => p.RemainingAmount).HasPrecision(18, 4);
            e.Property(x => x.SettlementStatus)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        });
        modelBuilder.Entity<SalesQuotation>(e =>
        {
            e.Property(p => p.SubTotal).HasPrecision(18, 4);
            e.Property(p => p.DiscountAmount).HasPrecision(18, 4);
            e.Property(p => p.TaxAmount).HasPrecision(18, 4);
            e.Property(p => p.TotalAmount).HasPrecision(18, 4);
        });
        modelBuilder.Entity<SalesInvoiceLine>(e =>
        {
            e.Property(p => p.Quantity).HasPrecision(18, 4);
            e.Property(p => p.UnitPrice).HasPrecision(18, 4);
            e.Property(p => p.DiscountPercent).HasPrecision(18, 4);
            e.Property(p => p.DiscountAmount).HasPrecision(18, 4);
            e.Property(p => p.TaxPercent).HasPrecision(18, 4);
            e.Property(p => p.TaxAmount).HasPrecision(18, 4);
            e.Property(p => p.LineTotal).HasPrecision(18, 4);
        });
        modelBuilder.Entity<SalesReturn>(e =>
        {
            e.Property(p => p.TotalAmount).HasPrecision(18, 4);
        });
        modelBuilder.Entity<SalesReturnLine>(e =>
        {
            e.Property(p => p.ReturnQuantity).HasPrecision(18, 4);
            e.Property(p => p.UnitPrice).HasPrecision(18, 4);
            e.Property(p => p.OriginalItemCost).HasPrecision(18, 4);
        });
        modelBuilder.Entity<CreditNote>(e => e.Property(p => p.Amount).HasPrecision(18, 4));

        modelBuilder.Entity<PurchaseInvoice>(e =>
        {
            e.Property(p => p.SubTotal).HasPrecision(18, 4);
            e.Property(p => p.DiscountAmount).HasPrecision(18, 4);
            e.Property(p => p.TaxAmount).HasPrecision(18, 4);
            e.Property(p => p.TotalAmount).HasPrecision(18, 4);
            e.Property(p => p.PaidAmount).HasPrecision(18, 4);
            e.Property(p => p.RemainingAmount).HasPrecision(18, 4);
        });
        modelBuilder.Entity<PurchaseOrder>(e =>
        {
            e.Property(p => p.SubTotal).HasPrecision(18, 4);
            e.Property(p => p.TaxAmount).HasPrecision(18, 4);
            e.Property(p => p.TotalAmount).HasPrecision(18, 4);
            e.HasMany(p => p.Lines)
                .WithOne(l => l.PurchaseOrder)
                .HasForeignKey(l => l.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<PurchaseOrderLine>(e =>
        {
            e.HasIndex(p => new { p.PurchaseOrderId, p.ItemId });
            e.Property(p => p.Quantity).HasPrecision(18, 4);
            e.Property(p => p.UnitCost).HasPrecision(18, 4);
            e.Property(p => p.TaxPercent).HasPrecision(18, 4);
            e.Property(p => p.TaxAmount).HasPrecision(18, 4);
            e.Property(p => p.LineTotal).HasPrecision(18, 4);
        });
        modelBuilder.Entity<PurchaseInvoiceLine>(e =>
        {
            e.Property(p => p.Quantity).HasPrecision(18, 4);
            e.Property(p => p.UnitCost).HasPrecision(18, 4);
            e.Property(p => p.DiscountPercent).HasPrecision(18, 4);
            e.Property(p => p.DiscountAmount).HasPrecision(18, 4);
            e.Property(p => p.TaxPercent).HasPrecision(18, 4);
            e.Property(p => p.TaxAmount).HasPrecision(18, 4);
            e.Property(p => p.LineTotal).HasPrecision(18, 4);
        });
        modelBuilder.Entity<PurchaseReturn>(e =>
        {
            e.Property(p => p.TotalAmount).HasPrecision(18, 4);
        });
        modelBuilder.Entity<PurchaseReturnLine>(e =>
        {
            e.Property(p => p.ReturnQuantity).HasPrecision(18, 4);
            e.Property(p => p.UnitCost).HasPrecision(18, 4);
        });
        modelBuilder.Entity<DebitNote>(e => e.Property(p => p.Amount).HasPrecision(18, 4));

        modelBuilder.Entity<InventoryTransaction>(e =>
        {
            e.Property(p => p.Quantity).HasPrecision(18, 4);
            e.Property(p => p.UnitCost).HasPrecision(18, 4);
            e.Property(p => p.TotalCost).HasPrecision(18, 4);
        });
        modelBuilder.Entity<GoodsReceiptNoteLine>(e =>
        {
            e.Property(p => p.ReceivedQuantity).HasPrecision(18, 4);
        });
        modelBuilder.Entity<DeliveryNoteLine>(e =>
        {
            e.Property(p => p.DeliveredQuantity).HasPrecision(18, 4);
        });
        modelBuilder.Entity<ItemBatch>(e =>
        {
            e.Property(p => p.InitialQuantity).HasPrecision(18, 4);
            e.Property(p => p.CurrentQuantity).HasPrecision(18, 4);
        });
        modelBuilder.Entity<BankTransfer>(e =>
        {
            e.Property(p => p.Amount).HasPrecision(18, 4);
        });

        modelBuilder.Entity<TreasuryEntities.BankReconciliation>(e =>
        {
            e.Property(p => p.StatementEndingBalance).HasPrecision(18, 4);
            e.Property(p => p.ClearedBalance).HasPrecision(18, 4);
        });

        modelBuilder.Entity<TreasuryEntities.BankReconciliationLine>(e =>
        {
            e.Property(p => p.Amount).HasPrecision(18, 4);
        });

        modelBuilder.Entity<InventoryAdjustment>(e =>
        {
            e.Property(p => p.Quantity).HasPrecision(18, 4);
        });
        modelBuilder.Entity<InventoryBalance>(e =>
        {
            e.ToTable("InventoryBalances", "inventory");
            e.HasKey(x => x.Id);
            e.Property(p => p.AvailableQuantity).HasPrecision(18, 4).IsRequired();
            e.Property(p => p.OnHandQuantity).HasPrecision(18, 4).IsRequired();
            e.Property(e => e.RowVersion).IsRowVersion();
            
            e.HasIndex(x => new { x.TenantId, x.ItemId, x.WarehouseId }).IsUnique();
            
            e.HasOne(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Warehouse)
                 .WithMany()
                 .HasForeignKey(x => x.WarehouseId)
                 .OnDelete(DeleteBehavior.Restrict);
         });
         modelBuilder.Entity<InventoryFIFOLayer>(e =>
         {
             e.ToTable("InventoryFIFOLayers", "inventory");
             e.HasKey(x => x.Id);
             e.Property(p => p.OriginalQuantity).HasPrecision(18, 4).IsRequired();
             e.Property(p => p.RemainingQuantity).HasPrecision(18, 4).IsRequired();
             e.Property(p => p.UnitCost).HasPrecision(18, 4).IsRequired();
             
             e.HasIndex(x => new { x.TenantId, x.ItemId, x.WarehouseId });
             e.HasIndex(x => x.ReceivedDate);
             
             e.HasOne(x => x.Item)
                 .WithMany()
                 .HasForeignKey(x => x.ItemId)
                 .OnDelete(DeleteBehavior.Restrict);

             e.HasOne(x => x.Warehouse)
                 .WithMany()
                 .HasForeignKey(x => x.WarehouseId)
                 .OnDelete(DeleteBehavior.Restrict);
         });
        modelBuilder.Entity<JournalEntry>(e =>
        {
            e.Property(p => p.TotalDebit).HasPrecision(18, 4);
            e.Property(p => p.TotalCredit).HasPrecision(18, 4);
            e.Property(p => p.CurrencyCode).HasMaxLength(10).IsRequired();
            e.Property(p => p.ExchangeRate).HasPrecision(18, 6);
        });

        modelBuilder.Entity<JournalEntryLine>(e =>
        {
            e.Property(p => p.DebitAmount).HasPrecision(18, 4);
            e.Property(p => p.CreditAmount).HasPrecision(18, 4);
            e.Property(p => p.BaseDebitAmount).HasPrecision(18, 4);
            e.Property(p => p.BaseCreditAmount).HasPrecision(18, 4);
            e.Property(p => p.AccountCode).HasMaxLength(50).IsRequired();
            e.Property(p => p.AccountName).HasMaxLength(200).IsRequired();
        });
        modelBuilder.Entity<Currency>(e => e.HasIndex(p => p.Code).IsUnique());
        modelBuilder.Entity<ExchangeRate>(e => e.Property(p => p.Rate).HasPrecision(18, 6));
        modelBuilder.Entity<Customer>(e =>
        {
            e.Property(p => p.CreditLimit).HasPrecision(18, 4);
            e.Property(p => p.OpeningBalance).HasPrecision(18, 4);
            e.Property(p => p.Balance).HasPrecision(18, 4);
        });
        modelBuilder.Entity<Supplier>(e =>
        {
            e.Property(p => p.CreditLimit).HasPrecision(18, 4);
            e.Property(p => p.OpeningBalance).HasPrecision(18, 4);
            e.Property(p => p.Balance).HasPrecision(18, 4);
        });
        modelBuilder.Entity<Item>(e =>
        {
            e.Property(p => p.BuyPrice).HasPrecision(18, 4);
            e.Property(p => p.LastBuyPrice).HasPrecision(18, 4);
            e.Property(p => p.AverageCost).HasPrecision(18, 4);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.Property(p => p.BasicSalary).HasPrecision(18, 4);
            e.Property(p => p.HousingAllowance).HasPrecision(18, 4);
            e.Property(p => p.TransportationAllowance).HasPrecision(18, 4);
        });

        modelBuilder.Entity<PayrollTransaction>(e =>
        {
            e.Property(p => p.BasicSalary).HasPrecision(18, 4);
            e.Property(p => p.Allowances).HasPrecision(18, 4);
            e.Property(p => p.Deductions).HasPrecision(18, 4);
            e.Property(p => p.NetSalary).HasPrecision(18, 4);
        });

        // Treasury
        modelBuilder.Entity<ReceiptVoucher>(e => e.Property(p => p.Amount).HasPrecision(18, 4));
        modelBuilder.Entity<PaymentVoucher>(e => e.Property(p => p.Amount).HasPrecision(18, 4));
        modelBuilder.Entity<BankTransfer>(e => e.Property(p => p.Amount).HasPrecision(18, 4));
        modelBuilder.Entity<InvoicePayment>(e =>
        {
            e.ToTable("InvoicePayments", "treasury");
            e.HasKey(x => x.Id);
            e.Property(p => p.AllocatedAmount).HasPrecision(18, 4).IsRequired();
            e.Property(x => x.SettlementDate).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);

            // Index Ù„Ù„Ø¨Ø­Ø« Ø§Ù„Ø³Ø±ÙŠØ¹
            e.HasIndex(x => new { x.TenantId, x.SalesInvoiceId })
                .HasDatabaseName("IX_InvoicePayments_TenantId_SalesInvoiceId");

            e.HasIndex(x => new { x.TenantId, x.ReceiptVoucherId })
                .HasDatabaseName("IX_InvoicePayments_TenantId_ReceiptVoucherId");

            // Ù…Ù†Ø¹ Ø§Ù„ØªØ³ÙˆÙŠØ© Ø§Ù„Ù…Ø²Ø¯ÙˆØ¬Ø© Ù„Ù†ÙØ³ Ø§Ù„Ø²ÙˆØ¬
            e.HasIndex(x => new { x.ReceiptVoucherId, x.SalesInvoiceId })
                .IsUnique()
                .HasDatabaseName("UX_InvoicePayments_Receipt_Invoice");

            // Ø§Ù„Ø¹Ù„Ø§Ù‚Ø§Øª
            e.HasOne(x => x.SalesInvoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(x => x.SalesInvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ReceiptVoucher)
                .WithMany(rv => rv.InvoicePayments)
                .HasForeignKey(x => x.ReceiptVoucherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Manufacturing
        modelBuilder.Entity<EnterpriseERP.Domain.Entities.Manufacturing.RawMaterial>(e =>
        {
            e.Property(p => p.StockLevel).HasPrecision(18, 4);
            e.Property(p => p.MinStockLevel).HasPrecision(18, 4);
            e.Property(p => p.CostPerUnit).HasPrecision(18, 4);
        });
        modelBuilder.Entity<EnterpriseERP.Domain.Entities.Manufacturing.BillOfMaterialsLine>(e =>
        {
            e.Property(p => p.Quantity).HasPrecision(18, 4);
        });
        modelBuilder.Entity<EnterpriseERP.Domain.Entities.Manufacturing.BillOfMaterials>(e =>
        {
            e.Property(p => p.LaborCostPerHour).HasPrecision(18, 4);
            e.Property(p => p.MachineOverheadPerHour).HasPrecision(18, 4);
            e.Property(p => p.TotalEstimatedCost).HasPrecision(18, 4);
            e.HasMany(p => p.Lines).WithOne(l => l.BillOfMaterials).HasForeignKey(l => l.BillOfMaterialsId);
            e.HasMany(p => p.Stages).WithOne(s => s.BillOfMaterials).HasForeignKey(s => s.BillOfMaterialsId);
        });
        modelBuilder.Entity<ProductionOrderMaterial>(e =>
        {
            e.Property(p => p.PlannedQuantity).HasPrecision(18, 4);
            e.Property(p => p.ActualQuantity).HasPrecision(18, 4);
        });

        modelBuilder.Entity<ProductionOrderStage>(e =>
        {
            e.Property(p => p.EstimatedHours).HasPrecision(18, 2);
            e.Property(p => p.ActualHours).HasPrecision(18, 2);
            e.Property(p => p.CostPerHour).HasPrecision(18, 4);
            e.HasOne(p => p.ProductionOrder).WithMany(o => o.Stages).HasForeignKey(p => p.ProductionOrderId);
        });
        modelBuilder.Entity<EnterpriseERP.Domain.Entities.Manufacturing.ProductionStage>(e =>
        {
            e.Property(p => p.CostPerHour).HasPrecision(18, 4);
            e.Property(p => p.EstimatedHours).HasPrecision(18, 4);
        });
        modelBuilder.Entity<EnterpriseERP.Domain.Entities.Manufacturing.ProductionOrder>(e =>
        {
            e.Property(p => p.Quantity).HasPrecision(18, 4);
            e.Property(p => p.ProducedQuantity).HasPrecision(18, 4);
            e.Property(p => p.TotalMaterialCost).HasPrecision(18, 4);
            e.Property(p => p.TotalLaborCost).HasPrecision(18, 4);
        });
        modelBuilder.Entity<EnterpriseERP.Domain.Entities.Manufacturing.ProductionOrderMaterial>(e =>
        {
            e.Property(p => p.PlannedQuantity).HasPrecision(18, 4);
            e.Property(p => p.ActualQuantity).HasPrecision(18, 4);
        });

        modelBuilder.Entity<WorkCenterCalendar>(e =>
        {
            e.HasOne(p => p.WorkCenter).WithMany().HasForeignKey(p => p.WorkCenterId);
        });

        modelBuilder.Entity<WorkCenterException>(e =>
        {
            e.HasOne(p => p.WorkCenter).WithMany().HasForeignKey(p => p.WorkCenterId);
        });

        // Maintenance
        modelBuilder.Entity<EnterpriseERP.Domain.Entities.Maintenance.MaintenanceTechnician>(e =>
        {
            e.Property(p => p.HourlyRate).HasPrecision(18, 4);
        });
        modelBuilder.Entity<EnterpriseERP.Domain.Entities.Maintenance.MaintenanceWorkOrder>(e =>
        {
            e.Property(p => p.TotalPartsCost).HasPrecision(18, 4);
            e.Property(p => p.TotalLaborCost).HasPrecision(18, 4);
        });
        modelBuilder.Entity<EnterpriseERP.Domain.Entities.Maintenance.MaintenanceWorkOrderPart>(e =>
        {
            e.Property(p => p.Quantity).HasPrecision(18, 4);
            e.Property(p => p.UnitCost).HasPrecision(18, 4);
        });

        modelBuilder.Entity<AccountMapping>(e =>
        {
            e.HasIndex(x => x.PostingType).IsUnique();
        });

        // TenantInvoice decimal precision
        modelBuilder.Entity<TenantInvoice>(e =>
        {
            e.Property(p => p.Amount).HasPrecision(18, 4);
            e.HasIndex(x => new { x.TenantId, x.Status });
            e.HasIndex(x => new { x.TenantId, x.PeriodStart, x.PeriodEnd });
        });

        // RowVersion for concurrency control - RULE-SEC01: Ø§Ù„ÙƒÙŠØ§Ù†Ø§Øª Ø§Ù„Ø­Ø³Ø§Ø³Ø© ÙŠØ¬Ø¨ Ø£Ù† ØªØ­ØªÙˆÙŠ RowVersion
        modelBuilder.Entity<Branch>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<Customer>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<Supplier>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<Item>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<Warehouse>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<SalesInvoice>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<PurchaseInvoice>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<TenantInvoice>().Property(e => e.RowVersion).IsRowVersion();
        
        // Additional sensitive entities
        modelBuilder.Entity<JournalEntry>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<InventoryTransaction>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<PayrollTransaction>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<Account>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<AccountMapping>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<Currency>().Property(e => e.RowVersion).IsRowVersion();
        modelBuilder.Entity<ExchangeRate>().Property(e => e.RowVersion).IsRowVersion();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService?.UserId ?? "System";
        var currentTenantId = _currentUserService?.TenantId ?? Guid.Empty;
        var currentCompanyId = _currentUserService?.CompanyId ?? Guid.Empty;

        // Domain Validation: Prevent duplicate AccountMappings
        var newMappings = ChangeTracker.Entries<AccountMapping>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        foreach (var mapping in newMappings)
        {
            if (await AccountMappings.AnyAsync(m => m.PostingType == mapping.PostingType, cancellationToken))
            {
                throw new AccountingDomainException($"Duplicate account mapping found for PostingType: '{mapping.PostingType}'. Each posting type must have exactly one account mapping.");
            }

            if (newMappings.Count(m => m.PostingType == mapping.PostingType) > 1)
            {
                throw new AccountingDomainException($"Duplicate account mapping for PostingType: '{mapping.PostingType}' found in the current transaction.");
            }
        }

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUserId;

                    // Security Enforcement: Prevent adding tenant-specific data without a tenant context
                    if (currentTenantId == Guid.Empty && entry.Entity.TenantId == Guid.Empty && entry.Entity is not Tenant)
                    {
                        throw new InvalidOperationException($"Security Breach: Attempted to save {entry.Entity.GetType().Name} without a valid TenantId context.");
                    }

                    if (entry.Entity.TenantId == Guid.Empty)
                    {
                        entry.Entity.TenantId = currentTenantId;
                    }
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = currentUserId;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ICompanyEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.CompanyId == Guid.Empty)
            {
                entry.Entity.CompanyId = currentCompanyId;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }

        var auditEntries = new List<AuditLog>();
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog)
                continue;

            var entityType = entry.Entity.GetType().Name;
            
            // Only audit critical entities
            if (entityType != nameof(JournalEntry) &&
                entityType != nameof(InventoryTransaction) &&
                entityType != nameof(SalesInvoice) &&
                entityType != nameof(PurchaseInvoice) &&
                entityType != nameof(PayrollTransaction) &&
                entityType != nameof(Account) &&
                entityType != nameof(AccountMapping) &&
                entityType != nameof(Currency) &&
                entityType != nameof(ExchangeRate))
            {
                continue;
            }

            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => entry.State.ToString()
            };

            var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            var entityId = idProperty?.CurrentValue?.ToString() ?? Guid.Empty.ToString();

            // V2: Capture Changes (Before/After)
            var changes = new List<string>();
            if (entry.State == EntityState.Modified)
            {
                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified)
                    {
                        changes.Add($"{prop.Metadata.Name}: {prop.OriginalValue} -> {prop.CurrentValue}");
                    }
                }
            }

            auditEntries.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                EventType = $"{entityType}.{action}",
                EntityName = entityType,
                EntityId = Guid.TryParse(entityId, out var parsedId) ? parsedId : Guid.Empty,
                Action = action,
                Details = changes.Count > 0 
                    ? $"User {currentUserId} updated {entityType} {entityId}. Changes: {string.Join(", ", changes)}"
                    : $"User {currentUserId} {action} {entityType} with ID {entityId}.",
                OccurredOn = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId,
                TenantId = currentTenantId, // Ensure TenantId is captured in AuditLogs
                Hash = ComputeAuditHash($"{entityType}.{action}", entityId, currentUserId, currentTenantId)
            });
        }

        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
        }

        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var pendingEntries = ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Modified or EntityState.Deleted)
                .Select(e =>
                {
                    var key = e.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue;
                    return $"{e.Entity.GetType().Name}:{key}:{e.State}";
                });

            throw new DbUpdateConcurrencyException(
                $"Concurrency failure while saving entries: {string.Join(", ", pendingEntries)}",
                ex);
        }
    }

    private string ComputeAuditHash(string eventType, string entityId, string userId, Guid tenantId)
    {
        // RULE-AUDIT01: Compute SHA-256 hash for audit log integrity
        var data = $"{eventType}|{entityId}|{userId}|{tenantId}|{DateTime.UtcNow.Ticks}";
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}






