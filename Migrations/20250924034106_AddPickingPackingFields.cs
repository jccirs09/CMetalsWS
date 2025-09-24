using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddPickingPackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedToId",
                table: "PickingLists",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Destination",
                table: "PickingLists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualWeight",
                table: "PickingListItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoilId",
                table: "PickingListItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DamageNotes",
                table: "PickingListItems",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "PickingListItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Packed",
                table: "PickingListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PackedAt",
                table: "PickingListItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackedById",
                table: "PickingListItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackingMaterial",
                table: "PickingListItems",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackingNotes",
                table: "PickingListItems",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Picked",
                table: "PickingListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickedAt",
                table: "PickingListItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickedById",
                table: "PickingListItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "QualityChecked",
                table: "PickingListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "QualityCheckedAt",
                table: "PickingListItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QualityCheckedById",
                table: "PickingListItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_AssignedToId",
                table: "PickingLists",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingListItems_PackedById",
                table: "PickingListItems",
                column: "PackedById");

            migrationBuilder.CreateIndex(
                name: "IX_PickingListItems_PickedById",
                table: "PickingListItems",
                column: "PickedById");

            migrationBuilder.CreateIndex(
                name: "IX_PickingListItems_QualityCheckedById",
                table: "PickingListItems",
                column: "QualityCheckedById");

            migrationBuilder.AddForeignKey(
                name: "FK_PickingListItems_AspNetUsers_PackedById",
                table: "PickingListItems",
                column: "PackedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PickingListItems_AspNetUsers_PickedById",
                table: "PickingListItems",
                column: "PickedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PickingListItems_AspNetUsers_QualityCheckedById",
                table: "PickingListItems",
                column: "QualityCheckedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PickingLists_AspNetUsers_AssignedToId",
                table: "PickingLists",
                column: "AssignedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PickingListItems_AspNetUsers_PackedById",
                table: "PickingListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PickingListItems_AspNetUsers_PickedById",
                table: "PickingListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PickingListItems_AspNetUsers_QualityCheckedById",
                table: "PickingListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PickingLists_AspNetUsers_AssignedToId",
                table: "PickingLists");

            migrationBuilder.DropIndex(
                name: "IX_PickingLists_AssignedToId",
                table: "PickingLists");

            migrationBuilder.DropIndex(
                name: "IX_PickingListItems_PackedById",
                table: "PickingListItems");

            migrationBuilder.DropIndex(
                name: "IX_PickingListItems_PickedById",
                table: "PickingListItems");

            migrationBuilder.DropIndex(
                name: "IX_PickingListItems_QualityCheckedById",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "AssignedToId",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "Destination",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ActualWeight",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "CoilId",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "DamageNotes",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "Packed",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "PackedAt",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "PackedById",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "PackingMaterial",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "PackingNotes",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "Picked",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "PickedAt",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "PickedById",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "QualityChecked",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "QualityCheckedAt",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "QualityCheckedById",
                table: "PickingListItems");
        }
    }
}
