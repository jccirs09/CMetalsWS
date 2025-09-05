using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddPickingListHeaderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PickingLists_BranchId",
                table: "PickingLists");

            migrationBuilder.AddColumn<string>(
                name: "FOB",
                table: "PickingLists",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderDate",
                table: "PickingLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesRep",
                table: "PickingLists",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipTo",
                table: "PickingLists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingVia",
                table: "PickingLists",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoldTo",
                table: "PickingLists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_BranchId_SalesOrderNumber",
                table: "PickingLists",
                columns: new[] { "BranchId", "SalesOrderNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PickingLists_BranchId_SalesOrderNumber",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "FOB",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "SalesRep",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ShipTo",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ShippingVia",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "SoldTo",
                table: "PickingLists");

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_BranchId",
                table: "PickingLists",
                column: "BranchId");
        }
    }
}
