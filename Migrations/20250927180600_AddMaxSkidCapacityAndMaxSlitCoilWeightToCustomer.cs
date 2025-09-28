using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    public partial class AddMaxSkidCapacityAndMaxSlitCoilWeightToCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxSkidCapacity",
                table: "Customer",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxSlitCoilWeight",
                table: "Customer",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxSkidCapacity",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "MaxSlitCoilWeight",
                table: "Customer");
        }
    }
}