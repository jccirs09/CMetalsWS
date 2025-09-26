using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPickingAndPacking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualWeight",
                table: "PickingListItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ActualWeight",
                table: "PickingListItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);
        }
    }
}
