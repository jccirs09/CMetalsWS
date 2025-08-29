using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class InventoryAdditionSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Branches_BranchId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_ItemId_TagNumber_BranchId",
                table: "InventoryItems");

            migrationBuilder.RenameColumn(
                name: "Weight",
                table: "InventoryItems",
                newName: "Snapshot");

            migrationBuilder.AlterColumn<string>(
                name: "TagNumber",
                table: "InventoryItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "InventoryItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "InventoryItems",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<string>(
                name: "SnapshotLabel",
                table: "InventoryItems",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SnapshotUnit",
                table: "InventoryItems",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ItemId",
                table: "InventoryItems",
                column: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_ItemId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "SnapshotLabel",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "SnapshotUnit",
                table: "InventoryItems");

            migrationBuilder.RenameColumn(
                name: "Snapshot",
                table: "InventoryItems",
                newName: "Weight");

            migrationBuilder.AlterColumn<string>(
                name: "TagNumber",
                table: "InventoryItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "InventoryItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "InventoryItems",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ItemId_TagNumber_BranchId",
                table: "InventoryItems",
                columns: new[] { "ItemId", "TagNumber", "BranchId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Branches_BranchId",
                table: "InventoryItems",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
