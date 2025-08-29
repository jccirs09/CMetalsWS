using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfWorkOrderNumberToWorkOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfWorkOrderNumber",
                table: "WorkOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChildItemDescription",
                table: "ItemRelationship",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ParentItemDescription",
                table: "ItemRelationship",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfWorkOrderNumber",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ChildItemDescription",
                table: "ItemRelationship");

            migrationBuilder.DropColumn(
                name: "ParentItemDescription",
                table: "ItemRelationship");
        }
    }
}
