using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Branch_Relationship_Mappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Branches_BranchId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_PickingLists_Branches_BranchId",
                table: "PickingLists");

            migrationBuilder.DropForeignKey(
                name: "FK_PickingLists_Trucks_TruckId",
                table: "PickingLists");

            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_AspNetUsers_DriverId",
                table: "Trucks");

            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_Branches_BranchId",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_WorkOrderNumber",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_PickingLists_PickingListNumber",
                table: "PickingLists");

            migrationBuilder.DropIndex(
                name: "IX_Machines_Code",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Branches_Code",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "PickingListNumber",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "ItemCode",
                table: "PickingListItems");

            migrationBuilder.RenameColumn(
                name: "ScheduledDate",
                table: "PickingLists",
                newName: "ShipDate");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "PickingLists",
                newName: "OrderDate");

            migrationBuilder.AlterColumn<string>(
                name: "WorkOrderNumber",
                table: "WorkOrders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "WorkOrderItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "PickingLists",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesOrderNumber",
                table: "PickingLists",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShipToAddress",
                table: "PickingLists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingMethod",
                table: "PickingLists",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "PickingListItems",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "PickingListItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "ItemDescription",
                table: "PickingListItems",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItemId",
                table: "PickingListItems",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Length",
                table: "PickingListItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineNumber",
                table: "PickingListItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MachineId",
                table: "PickingListItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "PickingListItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Width",
                table: "PickingListItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Machines",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Branches",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_SalesOrderNumber",
                table: "PickingLists",
                column: "SalesOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickingListItems_MachineId",
                table: "PickingListItems",
                column: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Branches_BranchId",
                table: "Machines",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PickingListItems_Machines_MachineId",
                table: "PickingListItems",
                column: "MachineId",
                principalTable: "Machines",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PickingLists_Branches_BranchId",
                table: "PickingLists",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PickingLists_Trucks_TruckId",
                table: "PickingLists",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_AspNetUsers_DriverId",
                table: "Trucks",
                column: "DriverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_Branches_BranchId",
                table: "Trucks",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Branches_BranchId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_PickingListItems_Machines_MachineId",
                table: "PickingListItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PickingLists_Branches_BranchId",
                table: "PickingLists");

            migrationBuilder.DropForeignKey(
                name: "FK_PickingLists_Trucks_TruckId",
                table: "PickingLists");

            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_AspNetUsers_DriverId",
                table: "Trucks");

            migrationBuilder.DropForeignKey(
                name: "FK_Trucks_Branches_BranchId",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_PickingLists_SalesOrderNumber",
                table: "PickingLists");

            migrationBuilder.DropIndex(
                name: "IX_PickingListItems_MachineId",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "SalesOrderNumber",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ShipToAddress",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ShippingMethod",
                table: "PickingLists");

            migrationBuilder.DropColumn(
                name: "ItemDescription",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "LineNumber",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "MachineId",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "PickingListItems");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "PickingListItems");

            migrationBuilder.RenameColumn(
                name: "ShipDate",
                table: "PickingLists",
                newName: "ScheduledDate");

            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "PickingLists",
                newName: "CreatedDate");

            migrationBuilder.AlterColumn<string>(
                name: "WorkOrderNumber",
                table: "WorkOrders",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "WorkOrderItems",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "PickingLists",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickingListNumber",
                table: "PickingLists",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "PickingListItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "PickingListItems",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PickingListItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItemCode",
                table: "PickingListItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Machines",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Branches",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_WorkOrderNumber",
                table: "WorkOrders",
                column: "WorkOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_PickingListNumber",
                table: "PickingLists",
                column: "PickingListNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_Code",
                table: "Machines",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Code",
                table: "Branches",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Branches_BranchId",
                table: "Machines",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PickingLists_Branches_BranchId",
                table: "PickingLists",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PickingLists_Trucks_TruckId",
                table: "PickingLists",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_AspNetUsers_DriverId",
                table: "Trucks",
                column: "DriverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Trucks_Branches_BranchId",
                table: "Trucks",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
