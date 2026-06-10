using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIFRSCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "AccountingPeriods");

            migrationBuilder.RenameColumn(
                name: "IsClosed",
                table: "FiscalYears",
                newName: "IsCurrent");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AccountingPeriods",
                newName: "PeriodName");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "FiscalYears",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "FiscalYears",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "AccountMappings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AccountingPeriods",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "FiscalYears");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "FiscalYears");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "AccountMappings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AccountingPeriods");

            migrationBuilder.RenameColumn(
                name: "IsCurrent",
                table: "FiscalYears",
                newName: "IsClosed");

            migrationBuilder.RenameColumn(
                name: "PeriodName",
                table: "AccountingPeriods",
                newName: "Name");

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "AccountingPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
