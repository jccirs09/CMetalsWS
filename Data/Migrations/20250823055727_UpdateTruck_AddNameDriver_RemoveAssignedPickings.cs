using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTruck_AddNameDriver_RemoveAssignedPickings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Identifier",
                table: "Trucks",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "DriverId",
                table: "Trucks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Trucks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Trucks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_DriverId",
                table: "Trucks",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Identifier",
                table: "Trucks",
                column: "Identifier",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_AspNetUsers_DriverId",
                table: "Trucks",
                column: "DriverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_AspNetUsers_DriverId",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_DriverId",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_Identifier",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Trucks");

            migrationBuilder.AlterColumn<string>(
                name: "Identifier",
                table: "Trucks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);
        }
    }
}
