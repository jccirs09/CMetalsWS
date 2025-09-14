using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class ExtendWorkOrderForScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveCoilUsageId",
                table: "WorkOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEnd",
                table: "WorkOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStart",
                table: "WorkOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoilDescription",
                table: "WorkOrders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CoilInventoryId",
                table: "WorkOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoilItemId",
                table: "WorkOrders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoilLocationAtWOStart",
                table: "WorkOrders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoilMillRef",
                table: "WorkOrders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CoilSnapshotAt",
                table: "WorkOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoilWeightAtWOStartLbs",
                table: "WorkOrders",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedMinutes",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "WorkOrders",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkOrderCoilUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    CoilInventoryId = table.Column<int>(type: "int", nullable: false),
                    CoilTagNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CoilItemId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CoilDescription = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    StartWeightLbs = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    EndWeightLbs = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    FromLocation = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ToLocation = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<byte>(type: "tinyint", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderCoilUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderCoilUsages_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ActiveCoilUsageId",
                table: "WorkOrders",
                column: "ActiveCoilUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderCoilUsages_WorkOrderId",
                table: "WorkOrderCoilUsages",
                column: "WorkOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_WorkOrderCoilUsages_ActiveCoilUsageId",
                table: "WorkOrders",
                column: "ActiveCoilUsageId",
                principalTable: "WorkOrderCoilUsages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Custom Indexes
            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_Machine_Schedule",
                table: "WorkOrders",
                columns: new[] { "MachineId", "ScheduledStartDate", "ScheduledEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_CoilInventoryId",
                table: "WorkOrders",
                column: "CoilInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderCoilUsage_WorkOrderId_Sequence",
                table: "WorkOrderCoilUsages",
                columns: new[] { "WorkOrderId", "Sequence" });

            // Backfill Script
            migrationBuilder.Sql(@"
                -- Backfill EstimatedMinutes where it is 0
                UPDATE WorkOrders
                SET EstimatedMinutes = DATEDIFF(minute, ScheduledStartDate, ScheduledEndDate)
                WHERE EstimatedMinutes = 0 AND ScheduledStartDate IS NOT NULL AND ScheduledEndDate IS NOT NULL;

                -- Backfill CoilSnapshotAt for existing records
                UPDATE WorkOrders
                SET CoilSnapshotAt = CreatedDate
                WHERE CoilSnapshotAt IS NULL;

                -- Backfill CoilInventoryId from InventoryItems based on BranchId and TagNumber
                UPDATE wo
                SET wo.CoilInventoryId = ii.Id
                FROM WorkOrders wo
                JOIN InventoryItems ii ON wo.BranchId = ii.BranchId AND wo.TagNumber = ii.TagNumber
                WHERE wo.CoilInventoryId IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_WorkOrderCoilUsages_ActiveCoilUsageId",
                table: "WorkOrders");

            migrationBuilder.DropTable(
                name: "WorkOrderCoilUsages");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_ActiveCoilUsageId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ActiveCoilUsageId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ActualEnd",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ActualStart",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CoilDescription",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CoilInventoryId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CoilItemId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CoilLocationAtWOStart",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CoilMillRef",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CoilSnapshotAt",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CoilWeightAtWOStartLbs",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "EstimatedMinutes",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "WorkOrders");
        }
    }
}
