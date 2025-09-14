using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class OperationsDashboardSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEndDate",
                table: "WorkOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStartDate",
                table: "WorkOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedMinutes",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefaultSetupMins",
                table: "Machines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RateUnits",
                table: "Machines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RateValue",
                table: "Machines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "LoadStopEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoadId = table.Column<int>(type: "int", nullable: false),
                    StopSequence = table.Column<int>(type: "int", nullable: false),
                    ArriveUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DepartUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadStopEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadStopEvent_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderCoilUsage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    CoilInventoryId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CoilTagNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderCoilUsage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderCoilUsage_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoadStopEvent_LoadId",
                table: "LoadStopEvent",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderCoilUsage_WorkOrderId",
                table: "WorkOrderCoilUsage",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoadStopEvent");

            migrationBuilder.DropTable(
                name: "WorkOrderCoilUsage");

            migrationBuilder.DropColumn(
                name: "ActualEndDate",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ActualStartDate",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "EstimatedMinutes",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "DefaultSetupMins",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "RateUnits",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "RateValue",
                table: "Machines");
        }
    }
}
