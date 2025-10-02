using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeToDestinationRegion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "WorkOrders",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

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

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DestinationRegions",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CoordinatorId",
                table: "DestinationRegions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DestinationRegions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresPooling",
                table: "DestinationRegions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "DestinationRegions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DestinationRegions_CoordinatorId",
                table: "DestinationRegions",
                column: "CoordinatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DestinationRegions_AspNetUsers_CoordinatorId",
                table: "DestinationRegions",
                column: "CoordinatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DestinationRegions_AspNetUsers_CoordinatorId",
                table: "DestinationRegions");

            migrationBuilder.DropIndex(
                name: "IX_DestinationRegions_CoordinatorId",
                table: "DestinationRegions");

            migrationBuilder.DropColumn(
                name: "CoordinatorId",
                table: "DestinationRegions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DestinationRegions");

            migrationBuilder.DropColumn(
                name: "RequiresPooling",
                table: "DestinationRegions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "DestinationRegions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "WorkOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DestinationRegions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);
        }
    }
}
