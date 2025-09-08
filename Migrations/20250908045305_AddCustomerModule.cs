using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customer_LocationCode",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "Customer");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Customer",
                newName: "TollRoutesAllowed");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Customer",
                newName: "PlaceId");

            migrationBuilder.AlterColumn<string>(
                name: "DestinationRegionCategory",
                table: "Customer",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AccessRestrictions",
                table: "Customer",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Customer",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AppointmentRequired",
                table: "Customer",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BusinessHours",
                table: "Customer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Customer",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Customer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "Customer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "Customer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Customer",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomTags",
                table: "Customer",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryNotes",
                table: "Customer",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationGroupCategory",
                table: "Customer",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DockType",
                table: "Customer",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "FerryRequired",
                table: "Customer",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FullAddress",
                table: "Customer",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Customer",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LiftgateRequired",
                table: "Customer",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Customer",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Customer",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredTruckType",
                table: "Customer",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Customer",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Customer",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceTimeMinutes",
                table: "Customer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Street1",
                table: "Customer",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street2",
                table: "Customer",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeWindowEnd",
                table: "Customer",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeWindowStart",
                table: "Customer",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "Customer",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CityCentroid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityCentroid", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Active",
                table: "Customer",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_City",
                table: "Customer",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_DestinationGroupCategory",
                table: "Customer",
                column: "DestinationGroupCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_DestinationRegionCategory",
                table: "Customer",
                column: "DestinationRegionCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_PostalCode",
                table: "Customer",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Province",
                table: "Customer",
                column: "Province");

            migrationBuilder.CreateIndex(
                name: "IX_CityCentroid_City_Province",
                table: "CityCentroid",
                columns: new[] { "City", "Province" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CityCentroid");

            migrationBuilder.DropIndex(
                name: "IX_Customer_Active",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_City",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_DestinationGroupCategory",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_DestinationRegionCategory",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_PostalCode",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_Province",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "AccessRestrictions",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "AppointmentRequired",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "BusinessHours",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "CustomTags",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DeliveryNotes",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DestinationGroupCategory",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DockType",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "FerryRequired",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "FullAddress",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "LiftgateRequired",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "PreferredTruckType",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "ServiceTimeMinutes",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Street1",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Street2",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "TimeWindowEnd",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "TimeWindowStart",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "Customer");

            migrationBuilder.RenameColumn(
                name: "TollRoutesAllowed",
                table: "Customer",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "PlaceId",
                table: "Customer",
                newName: "Address");

            migrationBuilder.AlterColumn<int>(
                name: "DestinationRegionCategory",
                table: "Customer",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "Customer",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_LocationCode",
                table: "Customer",
                column: "LocationCode");
        }
    }
}
