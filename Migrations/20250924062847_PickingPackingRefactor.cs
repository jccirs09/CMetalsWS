using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class PickingPackingRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InventoryItemId",
                table: "PickingListItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickingListItems_InventoryItemId",
                table: "PickingListItems",
                column: "InventoryItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_PickingListItems_InventoryItems_InventoryItemId",
                table: "PickingListItems",
                column: "InventoryItemId",
                principalTable: "InventoryItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PickingListItems_InventoryItems_InventoryItemId",
                table: "PickingListItems");

            migrationBuilder.DropIndex(
                name: "IX_PickingListItems_InventoryItemId",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "InventoryItemId",
                table: "PickingListItems");
        }
    }
}
