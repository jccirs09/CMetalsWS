using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfParsingAndAssignmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasParseIssues",
                table: "PickingLists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PageCount",
                table: "PickingLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ParseNotes",
                table: "PickingLists",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawTextHash",
                table: "PickingLists",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "BuildingCategory",
                table: "PickingListItems",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<bool>(
                name: "HasTagLots",
                table: "PickingListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsAttention",
                table: "PickingListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasParseIssues",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "PageCount",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ParseNotes",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "RawTextHash",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "BuildingCategory",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "HasTagLots",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "NeedsAttention",
                table: "PickingListItems");
        }
    }
}
