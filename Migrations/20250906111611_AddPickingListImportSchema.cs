using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddPickingListImportSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Buyer",
                table: "PickingLists",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrintDateTime",
                table: "PickingLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PickingListImports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickingListId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourcePdfPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagesPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelUsed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickingListImports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickingListImports_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PickingListImports_PickingLists_PickingListId",
                        column: x => x.PickingListId,
                        principalTable: "PickingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PickingListPageImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickingListImportId = table.Column<int>(type: "int", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickingListPageImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickingListPageImages_PickingListImports_PickingListImportId",
                        column: x => x.PickingListImportId,
                        principalTable: "PickingListImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PickingListImports_BranchId",
                table: "PickingListImports",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingListImports_PickingListId",
                table: "PickingListImports",
                column: "PickingListId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingListPageImages_PickingListImportId",
                table: "PickingListPageImages",
                column: "PickingListImportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PickingListPageImages");

            migrationBuilder.DropTable(
                name: "PickingListImports");

            migrationBuilder.DropColumn(
                name: "Buyer",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "PrintDateTime",
                table: "PickingLists");
        }
    }
}
