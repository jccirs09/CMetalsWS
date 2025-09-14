using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadedByToPickingList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "PickingLists",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UploadedBy",
                table: "PickingLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "PickingListItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedBy",
                table: "PickingListItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "UploadedBy",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "AssignedBy",
                table: "PickingListItems");
        }
    }
}
