using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddDestinationRegionToTruckAndLoad_Corrected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DestinationRegionId",
                table: "Trucks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationRegionId",
                table: "Loads",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_DestinationRegionId",
                table: "Trucks",
                column: "DestinationRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_DestinationRegionId",
                table: "Loads",
                column: "DestinationRegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loads_DestinationRegions_DestinationRegionId",
                table: "Loads",
                column: "DestinationRegionId",
                principalTable: "DestinationRegions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_DestinationRegions_DestinationRegionId",
                table: "Trucks",
                column: "DestinationRegionId",
                principalTable: "DestinationRegions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loads_DestinationRegions_DestinationRegionId",
                table: "Loads");

            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_DestinationRegions_DestinationRegionId",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_DestinationRegionId",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Loads_DestinationRegionId",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "DestinationRegionId",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "DestinationRegionId",
                table: "Loads");
        }
    }
}
