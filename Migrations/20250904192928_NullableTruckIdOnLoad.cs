using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class NullableTruckIdOnLoad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loads_Trucks_TruckId",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "ShipDate",
                table: "PickingListItems");

            migrationBuilder.AlterColumn<int>(
                name: "TruckId",
                table: "Loads",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Loads_Trucks_TruckId",
                table: "Loads",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loads_Trucks_TruckId",
                table: "Loads");

            migrationBuilder.AddColumn<DateTime>(
                name: "ShipDate",
                table: "PickingListItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TruckId",
                table: "Loads",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Loads_Trucks_TruckId",
                table: "Loads",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
