using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePickingListModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FOB",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ShippingVia",
                table: "PickingLists");

            migrationBuilder.AddColumn<int>(
                name: "DestinationRegionId",
                table: "PickingLists",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_DestinationRegionId",
                table: "PickingLists",
                column: "DestinationRegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PickingLists_DestinationRegions_DestinationRegionId",
                table: "PickingLists",
                column: "DestinationRegionId",
                principalTable: "DestinationRegions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PickingLists_DestinationRegions_DestinationRegionId",
                table: "PickingLists");

            migrationBuilder.DropIndex(
                name: "IX_PickingLists_DestinationRegionId",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "DestinationRegionId",
                table: "PickingLists");

            migrationBuilder.AddColumn<string>(
                name: "FOB",
                table: "PickingLists",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingVia",
                table: "PickingLists",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);
        }
    }
}
