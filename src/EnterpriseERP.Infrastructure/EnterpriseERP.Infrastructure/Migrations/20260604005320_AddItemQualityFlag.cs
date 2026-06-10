using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItemQualityFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_SalesInvoices_SalesInvoiceId",
                table: "DeliveryNotes");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryNotes_SalesInvoiceId",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "WorkCenter",
                table: "ProductionOrderStage");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SwiftCode",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "SalesOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "SalesInvoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryNoteId",
                table: "SalesInvoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "ReceiptVouchers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentCost",
                table: "RawMaterials",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "PurchaseInvoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "GoodsReceiptLineId",
                table: "PurchaseInvoiceLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectTaskId",
                table: "PurchaseInvoiceLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseOrderLineId",
                table: "PurchaseInvoiceLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualExpenseCost",
                table: "ProjectTasks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualMaterialCost",
                table: "ProjectTasks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BudgetAmount",
                table: "ProjectTasks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommitmentAmount",
                table: "ProjectTasks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualExpenseCost",
                table: "Projects",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualMaterialCost",
                table: "Projects",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommitmentAmount",
                table: "Projects",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkCenterId",
                table: "ProductionOrderStage",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "ProductionOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "PaymentVouchers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "JournalEntryLines",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "JournalEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "JournalEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "JournalEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntryHash",
                table: "JournalEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "JournalEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreviousEntryHash",
                table: "JournalEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsQualityControlRequired",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Items",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "GoodsReceiptNotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GradeId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JobPositionId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SalesInvoiceId",
                table: "DeliveryNotes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "SalesInvoiceId1",
                table: "DeliveryNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalesOrderId",
                table: "DeliveryNotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SubBOMId",
                table: "BillOfMaterialsLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "BillOfMaterials",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "AccountingPeriods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosedBy",
                table: "AccountingPeriods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAPReconciled",
                table: "AccountingPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsARReconciled",
                table: "AccountingPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBankReconciled",
                table: "AccountingPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixedAssetsDepreciated",
                table: "AccountingPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInventoryReconciled",
                table: "AccountingPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPayrollPosted",
                table: "AccountingPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AccountingBankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BankStatementBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LedgerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_AccountingBankAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountingBankReconciliations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatementEndingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalReconciled = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingBankReconciliations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultDepreciationRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DefaultMethod = table.Column<int>(type: "int", nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentDepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HousingAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransportationAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAllowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsGOSIApplicable = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeGOSIPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompanyGOSIPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeContracts_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeGrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeGrades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalBasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAllowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxPercent = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequisitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequisitionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequisitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QualityChecklists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_QualityChecklists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Routings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToleranceRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    LowerPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpperPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LowerAbsolute = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpperAbsolute = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_ToleranceRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransferOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferOrders_Warehouses_FromWarehouseId",
                        column: x => x.FromWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransferOrders_Warehouses_ToWarehouseId",
                        column: x => x.ToWarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseBins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Zone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MaxCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentVolume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseBins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseBins_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkCenters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverheadRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EfficiencyFactor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_WorkCenters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankReconciliationLine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankReconciliationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JournalEntryLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsMatched = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankReconciliationLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankReconciliationLine_AccountingBankReconciliations_BankReconciliationId",
                        column: x => x.BankReconciliationId,
                        principalTable: "AccountingBankReconciliations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountingFixedAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcquisitionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchaseValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalvageValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccumulatedDepreciation = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepreciationMethod = table.Column<int>(type: "int", nullable: false),
                    DepreciationRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "int", nullable: false),
                    LastDepreciationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_AccountingFixedAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingFixedAssets_AssetCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AssetCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobPositions_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HousingAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransportationAllowance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherAllowances = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OvertimeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AbsenceDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoanDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GOSIDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollEntries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollEntries_PayrollRuns_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "PayrollRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequisitionLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseRequisitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedUnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequisitionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequisitionLines_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseRequisitionLines_PurchaseRequisitions_PurchaseRequisitionId",
                        column: x => x.PurchaseRequisitionId,
                        principalTable: "PurchaseRequisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualityChecklistItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Requirement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    QualityChecklistId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityChecklistItems_QualityChecklists_QualityChecklistId",
                        column: x => x.QualityChecklistId,
                        principalTable: "QualityChecklists",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QualityInspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChecklistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InspectorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityInspections_QualityChecklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalTable: "QualityChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FromBinId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToBinId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferOrderLines_TransferOrders_TransferOrderId",
                        column: x => x.TransferOrderId,
                        principalTable: "TransferOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoutingSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    OperationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SetupTimeMinutes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RunTimeMinutesPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoutingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutingSteps_Routings_RoutingId",
                        column: x => x.RoutingId,
                        principalTable: "Routings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoutingSteps_WorkCenters_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalTable: "WorkCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetDepreciationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FixedAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NewAccumulatedDepreciation = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetDepreciationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetDepreciationLogs_AccountingFixedAssets_FixedAssetId",
                        column: x => x.FixedAssetId,
                        principalTable: "AccountingFixedAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionResultItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChecklistItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Requirement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false),
                    Finding = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QualityInspectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionResultItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionResultItems_QualityInspections_QualityInspectionId",
                        column: x => x.QualityInspectionId,
                        principalTable: "QualityInspections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrderStage_WorkCenterId",
                table: "ProductionOrderStage",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVouchers_SupplierId",
                table: "PaymentVouchers",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_CompanyId",
                table: "JournalEntries",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_GradeId",
                table: "Employees",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_JobPositionId",
                table: "Employees",
                column: "JobPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNotes_SalesInvoiceId1",
                table: "DeliveryNotes",
                column: "SalesInvoiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNotes_SalesOrderId",
                table: "DeliveryNotes",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CompanyId",
                table: "Branches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialsLines_SubBOMId",
                table: "BillOfMaterialsLines",
                column: "SubBOMId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingFixedAssets_CategoryId",
                table: "AccountingFixedAssets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetDepreciationLogs_FixedAssetId",
                table: "AssetDepreciationLogs",
                column: "FixedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_BankReconciliationLine_BankReconciliationId",
                table: "BankReconciliationLine",
                column: "BankReconciliationId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeContracts_EmployeeId",
                table: "EmployeeContracts",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionResultItems_QualityInspectionId",
                table: "InspectionResultItems",
                column: "QualityInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_JobPositions_DepartmentId",
                table: "JobPositions",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntries_EmployeeId",
                table: "PayrollEntries",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollEntries_PayrollRunId",
                table: "PayrollEntries",
                column: "PayrollRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_ItemId",
                table: "PurchaseOrderLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderLines_PurchaseOrderId_ItemId",
                table: "PurchaseOrderLines",
                columns: new[] { "PurchaseOrderId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisitionLines_ItemId",
                table: "PurchaseRequisitionLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisitionLines_PurchaseRequisitionId",
                table: "PurchaseRequisitionLines",
                column: "PurchaseRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecklistItems_QualityChecklistId",
                table: "QualityChecklistItems",
                column: "QualityChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_ChecklistId",
                table: "QualityInspections",
                column: "ChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingSteps_RoutingId",
                table: "RoutingSteps",
                column: "RoutingId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingSteps_WorkCenterId",
                table: "RoutingSteps",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferOrderLines_TransferOrderId",
                table: "TransferOrderLines",
                column: "TransferOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferOrders_FromWarehouseId",
                table: "TransferOrders",
                column: "FromWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferOrders_ToWarehouseId",
                table: "TransferOrders",
                column: "ToWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseBins_WarehouseId",
                table: "WarehouseBins",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillOfMaterialsLines_BillOfMaterials_SubBOMId",
                table: "BillOfMaterialsLines",
                column: "SubBOMId",
                principalTable: "BillOfMaterials",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Companies_CompanyId",
                table: "Branches",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_SalesInvoices_SalesInvoiceId1",
                table: "DeliveryNotes",
                column: "SalesInvoiceId1",
                principalTable: "SalesInvoices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_SalesOrders_SalesOrderId",
                table: "DeliveryNotes",
                column: "SalesOrderId",
                principalTable: "SalesOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_EmployeeGrades_GradeId",
                table: "Employees",
                column: "GradeId",
                principalTable: "EmployeeGrades",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_JobPositions_JobPositionId",
                table: "Employees",
                column: "JobPositionId",
                principalTable: "JobPositions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_Companies_CompanyId",
                table: "JournalEntries",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentVouchers_Suppliers_SupplierId",
                table: "PaymentVouchers",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderStage_WorkCenters_WorkCenterId",
                table: "ProductionOrderStage",
                column: "WorkCenterId",
                principalTable: "WorkCenters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillOfMaterialsLines_BillOfMaterials_SubBOMId",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Companies_CompanyId",
                table: "Branches");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_SalesInvoices_SalesInvoiceId1",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_SalesOrders_SalesOrderId",
                table: "DeliveryNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_EmployeeGrades_GradeId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_JobPositions_JobPositionId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_Companies_CompanyId",
                table: "JournalEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentVouchers_Suppliers_SupplierId",
                table: "PaymentVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderStage_WorkCenters_WorkCenterId",
                table: "ProductionOrderStage");

            migrationBuilder.DropTable(
                name: "AccountingBankAccounts");

            migrationBuilder.DropTable(
                name: "AssetDepreciationLogs");

            migrationBuilder.DropTable(
                name: "BankReconciliationLine");

            migrationBuilder.DropTable(
                name: "EmployeeContracts");

            migrationBuilder.DropTable(
                name: "EmployeeGrades");

            migrationBuilder.DropTable(
                name: "InspectionResultItems");

            migrationBuilder.DropTable(
                name: "JobPositions");

            migrationBuilder.DropTable(
                name: "PayrollEntries");

            migrationBuilder.DropTable(
                name: "PurchaseOrderLines");

            migrationBuilder.DropTable(
                name: "PurchaseRequisitionLines");

            migrationBuilder.DropTable(
                name: "QualityChecklistItems");

            migrationBuilder.DropTable(
                name: "RoutingSteps");

            migrationBuilder.DropTable(
                name: "ToleranceRules");

            migrationBuilder.DropTable(
                name: "TransferOrderLines");

            migrationBuilder.DropTable(
                name: "WarehouseBins");

            migrationBuilder.DropTable(
                name: "AccountingFixedAssets");

            migrationBuilder.DropTable(
                name: "AccountingBankReconciliations");

            migrationBuilder.DropTable(
                name: "QualityInspections");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "PayrollRuns");

            migrationBuilder.DropTable(
                name: "PurchaseRequisitions");

            migrationBuilder.DropTable(
                name: "Routings");

            migrationBuilder.DropTable(
                name: "WorkCenters");

            migrationBuilder.DropTable(
                name: "TransferOrders");

            migrationBuilder.DropTable(
                name: "AssetCategories");

            migrationBuilder.DropTable(
                name: "QualityChecklists");

            migrationBuilder.DropIndex(
                name: "IX_ProductionOrderStage_WorkCenterId",
                table: "ProductionOrderStage");

            migrationBuilder.DropIndex(
                name: "IX_PaymentVouchers_SupplierId",
                table: "PaymentVouchers");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_CompanyId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_GradeId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_JobPositionId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryNotes_SalesInvoiceId1",
                table: "DeliveryNotes");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryNotes_SalesOrderId",
                table: "DeliveryNotes");

            migrationBuilder.DropIndex(
                name: "IX_Branches_CompanyId",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_BillOfMaterialsLines_SubBOMId",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "SwiftCode",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "DeliveryNoteId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "ReceiptVouchers");

            migrationBuilder.DropColumn(
                name: "CurrentCost",
                table: "RawMaterials");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "GoodsReceiptLineId",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ProjectTaskId",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderLineId",
                table: "PurchaseInvoiceLines");

            migrationBuilder.DropColumn(
                name: "ActualExpenseCost",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "ActualMaterialCost",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "BudgetAmount",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "CommitmentAmount",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "ActualExpenseCost",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ActualMaterialCost",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CommitmentAmount",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "WorkCenterId",
                table: "ProductionOrderStage");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "PaymentVouchers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "EntryHash",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "PreviousEntryHash",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "IsQualityControlRequired",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "GradeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "JobPositionId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SalesInvoiceId1",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "SalesOrderId",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "SubBOMId",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "ClosedBy",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "IsAPReconciled",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "IsARReconciled",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "IsBankReconciled",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "IsFixedAssetsDepreciated",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "IsInventoryReconciled",
                table: "AccountingPeriods");

            migrationBuilder.DropColumn(
                name: "IsPayrollPosted",
                table: "AccountingPeriods");

            migrationBuilder.AddColumn<string>(
                name: "WorkCenter",
                table: "ProductionOrderStage",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "SalesInvoiceId",
                table: "DeliveryNotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNotes_SalesInvoiceId",
                table: "DeliveryNotes",
                column: "SalesInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_SalesInvoices_SalesInvoiceId",
                table: "DeliveryNotes",
                column: "SalesInvoiceId",
                principalTable: "SalesInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
