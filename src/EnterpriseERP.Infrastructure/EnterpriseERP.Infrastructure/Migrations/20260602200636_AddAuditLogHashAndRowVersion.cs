using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogHashAndRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string[] tables = { "PayrollTransactions", "JournalEntries", "InventoryTransactions", "ExchangeRates", "Currencies", "Accounts", "AccountMappings" };
            foreach (var table in tables)
            {
                migrationBuilder.DropColumn(name: "RowVersion", table: table);
                migrationBuilder.AddColumn<byte[]>(
                    name: "RowVersion",
                    table: table,
                    type: "rowversion",
                    rowVersion: true,
                    nullable: false);
            }

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "AuditLogs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hash",
                table: "AuditLogs");

            string[] tables = { "PayrollTransactions", "JournalEntries", "InventoryTransactions", "ExchangeRates", "Currencies", "Accounts", "AccountMappings" };
            foreach (var table in tables)
            {
                migrationBuilder.DropColumn(name: "RowVersion", table: table);
                migrationBuilder.AddColumn<byte[]>(
                    name: "RowVersion",
                    table: table,
                    type: "varbinary(max)",
                    nullable: false,
                    defaultValue: new byte[0]);
            }
        }
    }
}
