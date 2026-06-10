namespace EnterpriseERP.Domain.Entities.Identity;

/// <summary>
/// Static class defining all system permissions.
/// Follows Module.Action convention.
/// </summary>
public static class Permissions
{
    // Branches
    public const string BranchesView = "Branches.View";
    public const string BranchesCreate = "Branches.Create";
    public const string BranchesEdit = "Branches.Edit";
    public const string BranchesDelete = "Branches.Delete";

    // Companies / Legal Entities
    public const string CompaniesView = "Companies.View";
    public const string CompaniesManage = "Companies.Manage";

    // Warehouses
    public const string WarehousesView = "Warehouses.View";
    public const string WarehousesCreate = "Warehouses.Create";
    public const string WarehousesEdit = "Warehouses.Edit";

    // Items
    public const string ItemsView = "Items.View";
    public const string ItemsCreate = "Items.Create";
    public const string ItemsEdit = "Items.Edit";
    public const string ItemsDelete = "Items.Delete";

    // Customers
    public const string CustomersView = "Customers.View";
    public const string CustomersCreate = "Customers.Create";
    public const string CustomersEdit = "Customers.Edit";
    public const string CustomersDelete = "Customers.Delete";

    // Suppliers
    public const string SuppliersView = "Suppliers.View";
    public const string SuppliersCreate = "Suppliers.Create";
    public const string SuppliersEdit = "Suppliers.Edit";
    public const string SuppliersDelete = "Suppliers.Delete";

    // Sales
    public const string SalesInvoicesView = "SalesInvoices.View";
    public const string SalesInvoicesCreate = "SalesInvoices.Create";
    public const string SalesInvoicesApprove = "SalesInvoices.Approve";
    public const string SalesInvoicesCancel = "SalesInvoices.Cancel";
    public const string SalesOrderView = "SalesOrders.View";
    public const string SalesOrderCreate = "SalesOrders.Create";
    public const string SalesOrderManage = "SalesOrders.Manage";

    // Purchases
    public const string PurchaseInvoicesView = "PurchaseInvoices.View";
    public const string PurchaseInvoicesCreate = "PurchaseInvoices.Create";
    public const string PurchaseInvoicesApprove = "PurchaseInvoices.Approve";

    // Accounting
    public const string AccountsView = "Accounts.View";
    public const string AccountsCreate = "Accounts.Create";
    public const string JournalEntriesView = "JournalEntries.View";
    public const string JournalEntriesCreate = "JournalEntries.Create";
    public const string IntercompanyView = "Intercompany.View";
    public const string IntercompanyManage = "Intercompany.Manage";
    public const string WorkflowsView = "Workflows.View";
    public const string WorkflowsManage = "Workflows.Manage";
    public const string ApprovalsView = "Approvals.View";
    public const string ApprovalsAct = "Approvals.Act";

    // Projects
    public const string ProjectsView = "Projects.View";
    public const string ProjectsManage = "Projects.Manage";
    public const string TimesheetsView = "Timesheets.View";
    public const string TimesheetsManage = "Timesheets.Manage";
    public const string TimesheetsApprove = "Timesheets.Approve";

    // Inventory
    public const string InventoryView = "Inventory.View";
    public const string InventoryManage = "Inventory.Manage";

    // Inventory Advanced
    public const string InventoryAdvancedView = "InventoryAdvanced.View";
    public const string InventoryAdvancedManage = "InventoryAdvanced.Manage";

    // Manufacturing
    public const string ManufacturingView = "Manufacturing.View";
    public const string ManufacturingManage = "Manufacturing.Manage";

    // Maintenance
    public const string MaintenanceView = "Maintenance.View";
    public const string MaintenanceManage = "Maintenance.Manage";

    // Inventory Reports
    public const string InventoryReportsView = "InventoryReports.View";

    public const string AccountingManage = "Accounting.Manage";

    // Financial Reports
    public const string FinancialReportsView = "FinancialReports.View";

    // HR & Payroll
    public const string EmployeesView = "Employees.View";
    public const string EmployeesCreate = "Employees.Create";
    public const string EmployeesEdit = "Employees.Edit";
    public const string DeductionsManage = "Deductions.Manage";
    public const string PayrollView = "Payroll.View";
    public const string PayrollProcess = "Payroll.Process";

    // Fixed Assets
    public const string FixedAssetsView = "FixedAssets.View";
    public const string FixedAssetsCreate = "FixedAssets.Create";
    public const string FixedAssetsDepreciate = "FixedAssets.Depreciate";

    // Lookups
    public const string LookupsView = "Lookups.View";

    // Budgets
    public const string BudgetsView = "Budgets.View";
    public const string BudgetsManage = "Budgets.Manage";

    // Audit
    public const string AuditLogsView = "AuditLogs.View";

    // User Management
    public const string UsersView = "Users.View";
    public const string UsersCreate = "Users.Create";
    public const string UsersEdit = "Users.Edit";
    public const string RolesManage = "Roles.Manage";

    // System
    public const string SystemMigrate = "System.Migrate";

    // Treasury
    public const string TreasuryView = "Treasury.View";
    public const string TreasuryManage = "Treasury.Manage";

    // Sales Returns
    public const string SalesReturnsView = "SalesReturns.View";
    public const string SalesReturnsCreate = "SalesReturns.Create";
    public const string SalesReturnsApprove = "SalesReturns.Approve";

    // Purchase Returns
    public const string PurchaseReturnsView = "PurchaseReturns.View";
    public const string PurchaseReturnsCreate = "PurchaseReturns.Create";
    public const string PurchaseReturnsApprove = "PurchaseReturns.Approve";

    // Fiscal Years
    public const string FiscalYearsView = "FiscalYears.View";
    public const string FiscalYearsManage = "FiscalYears.Manage";

    // Bank Reconciliation
    public const string BankReconciliationView = "BankReconciliation.View";
    public const string BankReconciliationManage = "BankReconciliation.Manage";

    // Logistics
    public const string LogisticsView = "Logistics.View";
    public const string LogisticsManage = "Logistics.Manage";

    // Ledger
    public const string LedgerView = "Ledger.View";

    // Opening Balances
    public const string OpeningBalancesView = "OpeningBalances.View";
    public const string OpeningBalancesImport = "OpeningBalances.Import";

    // CRM
    public const string LeadsView = "Leads.View";
    public const string LeadsCreate = "Leads.Create";
    public const string OpportunitiesView = "Opportunities.View";
    public const string OpportunitiesCreate = "Opportunities.Create";
}
