using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddDestinationGroupAndRegion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customer_DestinationGroupCategory",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_DestinationRegionCategory",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "CustomTags",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DestinationGroupCategory",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DestinationRegionCategory",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DockType",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "FerryRequired",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "LiftgateRequired",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "PlaceId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Timezone",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "TollRoutesAllowed",
                table: "Customer");

            migrationBuilder.RenameColumn(
                name: "TimeWindowStart",
                table: "Customer",
                newName: "ReceivingHourStart");

            migrationBuilder.RenameColumn(
                name: "TimeWindowEnd",
                table: "Customer",
                newName: "ReceivingHourEnd");

            migrationBuilder.AddColumn<int>(
                name: "DestinationGroupId",
                table: "Customer",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationRegionId",
                table: "Customer",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "DestinationGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinationGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DestinationRegions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinationRegions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customer_DestinationGroupId",
                table: "Customer",
                column: "DestinationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_DestinationRegionId",
                table: "Customer",
                column: "DestinationRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FirstName",
                table: "AspNetUsers",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FirstName_LastName",
                table: "AspNetUsers",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LastName",
                table: "AspNetUsers",
                column: "LastName");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_DestinationGroups_DestinationGroupId",
                table: "Customer",
                column: "DestinationGroupId",
                principalTable: "DestinationGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_DestinationRegions_DestinationRegionId",
                table: "Customer",
                column: "DestinationRegionId",
                principalTable: "DestinationRegions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_DestinationGroups_DestinationGroupId",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_DestinationRegions_DestinationRegionId",
                table: "Customer");

            migrationBuilder.DropTable(
                name: "DestinationGroups");

            migrationBuilder.DropTable(
                name: "DestinationRegions");

            migrationBuilder.DropIndex(
                name: "IX_Customer_DestinationGroupId",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_DestinationRegionId",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_FirstName_LastName",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DestinationGroupId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "DestinationRegionId",
                table: "Customer");

            migrationBuilder.RenameColumn(
                name: "ReceivingHourStart",
                table: "Customer",
                newName: "TimeWindowStart");

            migrationBuilder.RenameColumn(
                name: "ReceivingHourEnd",
                table: "Customer",
                newName: "TimeWindowEnd");

            migrationBuilder.AddColumn<string>(
                name: "CustomTags",
                table: "Customer",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationGroupCategory",
                table: "Customer",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationRegionCategory",
                table: "Customer",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DockType",
                table: "Customer",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "FerryRequired",
                table: "Customer",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Customer",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LiftgateRequired",
                table: "Customer",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Customer",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceId",
                table: "Customer",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timezone",
                table: "Customer",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "TollRoutesAllowed",
                table: "Customer",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_DestinationGroupCategory",
                table: "Customer",
                column: "DestinationGroupCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_DestinationRegionCategory",
                table: "Customer",
                column: "DestinationRegionCategory");
        }
    }
}
