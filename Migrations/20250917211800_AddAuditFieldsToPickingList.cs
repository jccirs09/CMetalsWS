using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToPickingList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModifiedById",
                table: "PickingLists",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "PickingLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScannedById",
                table: "PickingLists",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScannedDate",
                table: "PickingLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_ModifiedById",
                table: "PickingLists",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_ScannedById",
                table: "PickingLists",
                column: "ScannedById");

            migrationBuilder.AddForeignKey(
                name: "FK_PickingLists_AspNetUsers_ModifiedById",
                table: "PickingLists",
                column: "ModifiedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PickingLists_AspNetUsers_ScannedById",
                table: "PickingLists",
                column: "ScannedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PickingLists_AspNetUsers_ModifiedById",
                table: "PickingLists");

            migrationBuilder.DropForeignKey(
                name: "FK_PickingLists_AspNetUsers_ScannedById",
                table: "PickingLists");

            migrationBuilder.DropIndex(
                name: "IX_PickingLists_ModifiedById",
                table: "PickingLists");

            migrationBuilder.DropIndex(
                name: "IX_PickingLists_ScannedById",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ScannedById",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ScannedDate",
                table: "PickingLists");
        }
    }
}
