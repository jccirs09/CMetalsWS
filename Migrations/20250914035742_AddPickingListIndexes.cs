using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddPickingListIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Index for deduplication
            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_BranchId_RawTextHash",
                table: "PickingLists",
                columns: new[] { "BranchId", "RawTextHash" },
                unique: true,
                filter: "[RawTextHash] IS NOT NULL");

            // Index for querying items for assignment/display
            migrationBuilder.CreateIndex(
                name: "IX_PickingListItems_PickingListId_MachineId_BuildingCategory_Status",
                table: "PickingListItems",
                columns: new[] { "PickingListId", "MachineId", "BuildingCategory", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PickingLists_BranchId_RawTextHash",
                table: "PickingLists");

            migrationBuilder.DropIndex(
                name: "IX_PickingListItems_PickingListId_MachineId_BuildingCategory_Status",
                table: "PickingListItems");
        }
    }
}
