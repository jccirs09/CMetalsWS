using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchWorkingHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "Branches",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "Branches",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Branches");
        }
    }
}
