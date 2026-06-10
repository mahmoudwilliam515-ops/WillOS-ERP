using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancyAndBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_SubscriptionPlans_SubscriptionPlanId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_SubscriptionPlanId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "DatabaseConnectionString",
                table: "Tenants",
                newName: "TimeZone");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "ProductionStages",
                newName: "SequenceOrder");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Warehouses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "AdminEmail",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConnectionString",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DatabaseName",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultCurrency",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultLanguage",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FiscalYearStart",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsers",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Settings",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionPlanId1",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionStartDate",
                table: "Tenants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Suppliers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualPrice",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Features",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SubscriptionPlans",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SalesReturns",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SalesReturnLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SalesQuotations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SalesOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SalesOrderLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SalesInvoices");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "SalesInvoices",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SalesInvoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "ItemCost",
                table: "SalesInvoiceLines",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ReceiptVouchers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "RawMaterials",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PurchaseReturns",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PurchaseReturnLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PurchaseOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PurchaseInvoices");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PurchaseInvoices",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PurchaseInvoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BillOfMaterialsId",
                table: "ProductionStages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedHours",
                table: "ProductionStages",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductionStages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "WorkCenter",
                table: "ProductionStages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ProductionOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PayrollTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PaymentVouchers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "PartyBalances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Opportunities",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "OpeningBalanceSessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "MaintenanceWorkOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "MaintenanceTechnicians",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Leads",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "AccountName",
                table: "JournalEntryLines",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AccountCode",
                table: "JournalEntryLines",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "BaseCreditAmount",
                table: "JournalEntryLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseDebitAmount",
                table: "JournalEntryLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "JournalEntries",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "JournalEntries",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "JournalEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ItemSerials",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Items",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "ItemBatches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "InventoryTransfers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "InventoryTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "InventoryReservations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "InventoryAdjustments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "GoodsReceiptNotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "GoodsReceiptLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRevaluationDate",
                table: "FixedAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RevaluationAmount",
                table: "FixedAssets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "FixedAssets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalEstimatedUnits",
                table: "FixedAssets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitsProducedSoFar",
                table: "FixedAssets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "FiscalYears",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "EmployeeDeductions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DepreciationTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DeliveryNotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DeliveryNoteLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DebitNotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "CycleCounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "CycleCountLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "CreditNotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "CostCenters",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "CashAccounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Budgets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BudgetLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "BillOfMaterials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LaborCostPerHour",
                table: "BillOfMaterials",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MachineOverheadPerHour",
                table: "BillOfMaterials",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BillOfMaterials",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalEstimatedCost",
                table: "BillOfMaterials",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "BillOfMaterials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BankTransfers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BankReconciliations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BankReconciliationLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BankAccounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Attendances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "CashFlowCategory",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Accounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "AccountingPeriods",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "AccountMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountMappings_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBaseCurrency = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoicePayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiptVoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentVoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SalesInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PurchaseInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoicePayments_PaymentVouchers_PaymentVoucherId",
                        column: x => x.PaymentVoucherId,
                        principalTable: "PaymentVouchers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoicePayments_PurchaseInvoices_PurchaseInvoiceId",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoicePayments_ReceiptVouchers_ReceiptVoucherId",
                        column: x => x.ReceiptVoucherId,
                        principalTable: "ReceiptVouchers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoicePayments_SalesInvoices_SalesInvoiceId",
                        column: x => x.SalesInvoiceId,
                        principalTable: "SalesInvoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductionOrderStage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualHours = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostPerHour = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    WorkCenter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MachineId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperatorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionOrderStage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionOrderStage_ProductionOrders_ProductionOrderId",
                        column: x => x.ProductionOrderId,
                        principalTable: "ProductionOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantInvoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantInvoices_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantInvoices_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeRates_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_SubscriptionPlanId1",
                table: "Tenants",
                column: "SubscriptionPlanId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStages_BillOfMaterialsId",
                table: "ProductionStages",
                column: "BillOfMaterialsId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountMappings_AccountId",
                table: "AccountMappings",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountMappings_PostingType",
                table: "AccountMappings",
                column: "PostingType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_CurrencyId",
                table: "ExchangeRates",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_PaymentVoucherId",
                table: "InvoicePayments",
                column: "PaymentVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_PurchaseInvoiceId",
                table: "InvoicePayments",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_ReceiptVoucherId",
                table: "InvoicePayments",
                column: "ReceiptVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_SalesInvoiceId",
                table: "InvoicePayments",
                column: "SalesInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderStage_ProductionOrderId",
                table: "ProductionOrderStage",
                column: "ProductionOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantInvoices_SubscriptionPlanId",
                table: "TenantInvoices",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantInvoices_TenantId_PeriodStart_PeriodEnd",
                table: "TenantInvoices",
                columns: new[] { "TenantId", "PeriodStart", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantInvoices_TenantId_Status",
                table: "TenantInvoices",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionStages_BillOfMaterials_BillOfMaterialsId",
                table: "ProductionStages",
                column: "BillOfMaterialsId",
                principalTable: "BillOfMaterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_SubscriptionPlans_SubscriptionPlanId1",
                table: "Tenants",
                column: "SubscriptionPlanId1",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionStages_BillOfMaterials_BillOfMaterialsId",
                table: "ProductionStages");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_SubscriptionPlans_SubscriptionPlanId1",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "AccountMappings");

            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "InvoicePayments");

            migrationBuilder.DropTable(
                name: "ProductionOrderStage");

            migrationBuilder.DropTable(
                name: "TenantInvoices");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_SubscriptionPlanId1",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_ProductionStages_BillOfMaterialsId",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "AdminEmail",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ConnectionString",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DatabaseName",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DefaultCurrency",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DefaultLanguage",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "FiscalYearStart",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "MaxUsers",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Settings",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId1",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SubscriptionStartDate",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "AnnualPrice",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "Features",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SalesReturnLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SalesQuotations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SalesOrderLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "ItemCost",
                table: "SalesInvoiceLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ReceiptVouchers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "RawMaterials");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PurchaseReturnLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "BillOfMaterialsId",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "WorkCenter",
                table: "ProductionStages");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PayrollTransactions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PaymentVouchers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PartyBalances");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "OpeningBalanceSessions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "MaintenanceWorkOrders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "MaintenanceTechnicians");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "BaseCreditAmount",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "BaseDebitAmount",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ItemSerials");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "ItemBatches");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "InventoryTransfers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "InventoryReservations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "InventoryAdjustments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "GoodsReceiptLines");

            migrationBuilder.DropColumn(
                name: "LastRevaluationDate",
                table: "FixedAssets");

            migrationBuilder.DropColumn(
                name: "RevaluationAmount",
                table: "FixedAssets");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FixedAssets");

            migrationBuilder.DropColumn(
                name: "TotalEstimatedUnits",
                table: "FixedAssets");

            migrationBuilder.DropColumn(
                name: "UnitsProducedSoFar",
                table: "FixedAssets");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FiscalYears");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EmployeeDeductions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DepreciationTransactions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DeliveryNoteLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DebitNotes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CycleCounts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CycleCountLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CreditNotes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CostCenters");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "CashAccounts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BudgetLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "LaborCostPerHour",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "MachineOverheadPerHour",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "TotalEstimatedCost",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BankTransfers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BankReconciliations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BankReconciliationLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CashFlowCategory",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AccountingPeriods");

            migrationBuilder.RenameColumn(
                name: "TimeZone",
                table: "Tenants",
                newName: "DatabaseConnectionString");

            migrationBuilder.RenameColumn(
                name: "SequenceOrder",
                table: "ProductionStages",
                newName: "Order");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tenants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "SalesInvoices",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "rowversion",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "PurchaseInvoices",
                type: "varbinary(max)",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "rowversion",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountName",
                table: "JournalEntryLines",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "AccountCode",
                table: "JournalEntryLines",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "AuditLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AuditLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_SubscriptionPlanId",
                table: "Tenants",
                column: "SubscriptionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_SubscriptionPlans_SubscriptionPlanId",
                table: "Tenants",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
