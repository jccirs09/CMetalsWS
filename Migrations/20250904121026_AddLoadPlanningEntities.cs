using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddLoadPlanningEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loads_Branches_BranchId",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "ReadyDate",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "ScheduledDate",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "ScheduledEnd",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "ScheduledStart",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "Destination",
                table: "LoadItems");

            migrationBuilder.RenameColumn(
                name: "ShippingMethod",
                table: "PickingLists",
                newName: "DestinationRegion");

            migrationBuilder.RenameColumn(
                name: "BranchId",
                table: "Loads",
                newName: "OriginBranchId");

            migrationBuilder.RenameIndex(
                name: "IX_Loads_BranchId",
                table: "Loads",
                newName: "IX_Loads_OriginBranchId");

            migrationBuilder.RenameColumn(
                name: "Weight",
                table: "LoadItems",
                newName: "ShippedWeight");

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingWeight",
                table: "PickingLists",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ShippingGroup",
                table: "PickingLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWeight",
                table: "PickingLists",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "LoadNumber",
                table: "Loads",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "DestinationBranchId",
                table: "Loads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoadType",
                table: "Loads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Loads",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippingDate",
                table: "Loads",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWeight",
                table: "Loads",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "PickingListId",
                table: "LoadItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "StopSequence",
                table: "LoadItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransferItemId",
                table: "LoadItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultDestinationRegion",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultShippingGroup",
                table: "Customer",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TransferItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SKU = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Loads_DestinationBranchId",
                table: "Loads",
                column: "DestinationBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadItems_TransferItemId",
                table: "LoadItems",
                column: "TransferItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoadItems_TransferItems_TransferItemId",
                table: "LoadItems",
                column: "TransferItemId",
                principalTable: "TransferItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Loads_Branches_DestinationBranchId",
                table: "Loads",
                column: "DestinationBranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Loads_Branches_OriginBranchId",
                table: "Loads",
                column: "OriginBranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoadItems_TransferItems_TransferItemId",
                table: "LoadItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Loads_Branches_DestinationBranchId",
                table: "Loads");

            migrationBuilder.DropForeignKey(
                name: "FK_Loads_Branches_OriginBranchId",
                table: "Loads");

            migrationBuilder.DropTable(
                name: "TransferItems");

            migrationBuilder.DropIndex(
                name: "IX_Loads_DestinationBranchId",
                table: "Loads");

            migrationBuilder.DropIndex(
                name: "IX_LoadItems_TransferItemId",
                table: "LoadItems");

            migrationBuilder.DropColumn(
                name: "RemainingWeight",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ShippingGroup",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "TotalWeight",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "DestinationBranchId",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "LoadType",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "ShippingDate",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "TotalWeight",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "StopSequence",
                table: "LoadItems");

            migrationBuilder.DropColumn(
                name: "TransferItemId",
                table: "LoadItems");

            migrationBuilder.DropColumn(
                name: "DefaultDestinationRegion",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DefaultShippingGroup",
                table: "Customer");

            migrationBuilder.RenameColumn(
                name: "DestinationRegion",
                table: "PickingLists",
                newName: "ShippingMethod");

            migrationBuilder.RenameColumn(
                name: "OriginBranchId",
                table: "Loads",
                newName: "BranchId");

            migrationBuilder.RenameIndex(
                name: "IX_Loads_OriginBranchId",
                table: "Loads",
                newName: "IX_Loads_BranchId");

            migrationBuilder.RenameColumn(
                name: "ShippedWeight",
                table: "LoadItems",
                newName: "Weight");

            migrationBuilder.AlterColumn<string>(
                name: "LoadNumber",
                table: "Loads",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyDate",
                table: "Loads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledDate",
                table: "Loads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledEnd",
                table: "Loads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledStart",
                table: "Loads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PickingListId",
                table: "LoadItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Destination",
                table: "LoadItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Loads_Branches_BranchId",
                table: "Loads",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
