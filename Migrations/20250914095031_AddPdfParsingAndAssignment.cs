using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfParsingAndAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PickingListPageImages");

            migrationBuilder.DropTable(
                name: "PickingListImports");

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

            migrationBuilder.AddColumn<string>(
                name: "SourceFileName",
                table: "PickingLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "PickingLists",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UploadedBy",
                table: "PickingLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "PickingListItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedBy",
                table: "PickingListItems",
                type: "nvarchar(max)",
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

            migrationBuilder.AddColumn<string>(
                name: "SalesNote",
                table: "PickingListItems",
                type: "nvarchar(max)",
                nullable: true);
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
                name: "SourceFileName",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "UploadedBy",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "AssignedBy",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "BuildingCategory",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "HasTagLots",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "NeedsAttention",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "SalesNote",
                table: "PickingListItems");

            migrationBuilder.CreateTable(
                name: "PickingListImports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    PickingListId = table.Column<int>(type: "int", nullable: true),
                    CompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagesPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelUsed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RawJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SalesOrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourcePdfPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
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
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false)
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
    }
}
