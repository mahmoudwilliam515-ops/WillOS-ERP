using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionOptimisticConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EInvoiceAuthority",
                table: "SalesInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EInvoiceQrPayload",
                table: "SalesInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EInvoiceResponseMessage",
                table: "SalesInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EInvoiceStatus",
                table: "SalesInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EInvoiceSubmittedAt",
                table: "SalesInvoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EInvoiceUuid",
                table: "SalesInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EInvoiceXmlHash",
                table: "SalesInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BalanceSheetClassification",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EInvoiceAuthority",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "EInvoiceQrPayload",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "EInvoiceResponseMessage",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "EInvoiceStatus",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "EInvoiceSubmittedAt",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "EInvoiceUuid",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "EInvoiceXmlHash",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "BalanceSheetClassification",
                table: "Accounts");
        }
    }
}
