using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturingExcellence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillOfMaterialsLines_Items_MaterialId",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderMaterials_Items_MaterialId",
                table: "ProductionOrderMaterials");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "BillOfMaterials");

            migrationBuilder.RenameColumn(
                name: "MinStock",
                table: "RawMaterials",
                newName: "MinStockLevel");

            migrationBuilder.RenameColumn(
                name: "SequenceOrder",
                table: "ProductionStages",
                newName: "Sequence");

            migrationBuilder.RenameColumn(
                name: "SequenceOrder",
                table: "ProductionOrderStage",
                newName: "Sequence");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "ProductionOrders",
                newName: "ActualStartDate");

            migrationBuilder.RenameColumn(
                name: "PlannedQuantity",
                table: "ProductionOrders",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "ProductionOrders",
                newName: "ActualEndDate");

            migrationBuilder.RenameColumn(
                name: "MaterialId",
                table: "ProductionOrderMaterials",
                newName: "RawMaterialId");

            migrationBuilder.RenameColumn(
                name: "ConsumedQuantity",
                table: "ProductionOrderMaterials",
                newName: "ActualQuantity");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrderMaterials_MaterialId",
                table: "ProductionOrderMaterials",
                newName: "IX_ProductionOrderMaterials_RawMaterialId");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "BillOfMaterialsLines",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "MaterialId",
                table: "BillOfMaterialsLines",
                newName: "RawMaterialId");

            migrationBuilder.RenameIndex(
                name: "IX_BillOfMaterialsLines_MaterialId",
                table: "BillOfMaterialsLines",
                newName: "IX_BillOfMaterialsLines_RawMaterialId");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "RawMaterials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ReorderPoint",
                table: "RawMaterials",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                table: "RawMaterials",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedEndDate",
                table: "ProductionOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedStartDate",
                table: "ProductionOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitCost",
                table: "ProductionOrderMaterials",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddColumn<decimal>(
                name: "ScrapQuantity",
                table: "ProductionOrderMaterials",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ScrapFactor",
                table: "BillOfMaterialsLines",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "BillOfMaterials",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BillOfMaterials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_BillOfMaterialsLines_RawMaterials_RawMaterialId",
                table: "BillOfMaterialsLines",
                column: "RawMaterialId",
                principalTable: "RawMaterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderMaterials_RawMaterials_RawMaterialId",
                table: "ProductionOrderMaterials",
                column: "RawMaterialId",
                principalTable: "RawMaterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillOfMaterialsLines_RawMaterials_RawMaterialId",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrderMaterials_RawMaterials_RawMaterialId",
                table: "ProductionOrderMaterials");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "RawMaterials");

            migrationBuilder.DropColumn(
                name: "ReorderPoint",
                table: "RawMaterials");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "RawMaterials");

            migrationBuilder.DropColumn(
                name: "PlannedEndDate",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "PlannedStartDate",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "ScrapQuantity",
                table: "ProductionOrderMaterials");

            migrationBuilder.DropColumn(
                name: "ScrapFactor",
                table: "BillOfMaterialsLines");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "BillOfMaterials");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BillOfMaterials");

            migrationBuilder.RenameColumn(
                name: "MinStockLevel",
                table: "RawMaterials",
                newName: "MinStock");

            migrationBuilder.RenameColumn(
                name: "Sequence",
                table: "ProductionStages",
                newName: "SequenceOrder");

            migrationBuilder.RenameColumn(
                name: "Sequence",
                table: "ProductionOrderStage",
                newName: "SequenceOrder");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "ProductionOrders",
                newName: "PlannedQuantity");

            migrationBuilder.RenameColumn(
                name: "ActualStartDate",
                table: "ProductionOrders",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "ActualEndDate",
                table: "ProductionOrders",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "RawMaterialId",
                table: "ProductionOrderMaterials",
                newName: "MaterialId");

            migrationBuilder.RenameColumn(
                name: "ActualQuantity",
                table: "ProductionOrderMaterials",
                newName: "ConsumedQuantity");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrderMaterials_RawMaterialId",
                table: "ProductionOrderMaterials",
                newName: "IX_ProductionOrderMaterials_MaterialId");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "BillOfMaterialsLines",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "RawMaterialId",
                table: "BillOfMaterialsLines",
                newName: "MaterialId");

            migrationBuilder.RenameIndex(
                name: "IX_BillOfMaterialsLines_RawMaterialId",
                table: "BillOfMaterialsLines",
                newName: "IX_BillOfMaterialsLines_MaterialId");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitCost",
                table: "ProductionOrderMaterials",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "BillOfMaterialsLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "BillOfMaterials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_BillOfMaterialsLines_Items_MaterialId",
                table: "BillOfMaterialsLines",
                column: "MaterialId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrderMaterials_Items_MaterialId",
                table: "ProductionOrderMaterials",
                column: "MaterialId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
