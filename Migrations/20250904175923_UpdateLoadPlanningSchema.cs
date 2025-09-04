using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLoadPlanningSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoadItems_TransferItems_TransferItemId",
                table: "LoadItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PickingLists_Trucks_TruckId",
                table: "PickingLists");

            migrationBuilder.DropIndex(
                name: "IX_PickingLists_TruckId",
                table: "PickingLists");

            migrationBuilder.DropIndex(
                name: "IX_LoadItems_TransferItemId",
                table: "LoadItems");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "DestinationRegion",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "SalesRep",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ShipDate",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ShipToAddress",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ShippingGroup",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "TruckId",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "LoadType",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "TransferItemId",
                table: "LoadItems");

            migrationBuilder.DropColumn(
                name: "DefaultDestinationRegion",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DefaultShippingGroup",
                table: "Customer");

            migrationBuilder.AlterColumn<decimal>(
                name: "PulledWeight",
                table: "PickingListItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShipDate",
                table: "PickingListItems",
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

            migrationBuilder.AddColumn<int>(
                name: "PickingListItemId",
                table: "LoadItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DestinationRegionCategory",
                table: "Customer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LoadItems_PickingListItemId",
                table: "LoadItems",
                column: "PickingListItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoadItems_PickingListItems_PickingListItemId",
                table: "LoadItems",
                column: "PickingListItemId",
                principalTable: "PickingListItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoadItems_PickingListItems_PickingListItemId",
                table: "LoadItems");

            migrationBuilder.DropIndex(
                name: "IX_LoadItems_PickingListItemId",
                table: "LoadItems");

            migrationBuilder.DropColumn(
                name: "ShipDate",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "PickingListItemId",
                table: "LoadItems");

            migrationBuilder.DropColumn(
                name: "DestinationRegionCategory",
                table: "Customer");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "PickingLists",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationRegion",
                table: "PickingLists",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PickingLists",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderDate",
                table: "PickingLists",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SalesRep",
                table: "PickingLists",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShipDate",
                table: "PickingLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipToAddress",
                table: "PickingLists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingGroup",
                table: "PickingLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TruckId",
                table: "PickingLists",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PulledWeight",
                table: "PickingListItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AddColumn<int>(
                name: "LoadType",
                table: "Loads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "PickingListId",
                table: "LoadItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_TruckId",
                table: "PickingLists",
                column: "TruckId");

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
                name: "FK_PickingLists_Trucks_TruckId",
                table: "PickingLists",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
