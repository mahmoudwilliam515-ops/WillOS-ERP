using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowEnhancementsPhase3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "WorkflowDefinitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CostCenterId",
                table: "WorkflowDefinitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EscalationRole",
                table: "WorkflowDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "WorkflowDefinitions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SlaHours",
                table: "WorkflowDefinitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DelegatedToUserId",
                table: "ApprovalRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "ApprovalRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEscalated",
                table: "ApprovalRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "EscalationRole",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "SlaHours",
                table: "WorkflowDefinitions");

            migrationBuilder.DropColumn(
                name: "DelegatedToUserId",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "ApprovalRequests");

            migrationBuilder.DropColumn(
                name: "IsEscalated",
                table: "ApprovalRequests");
        }
    }
}
