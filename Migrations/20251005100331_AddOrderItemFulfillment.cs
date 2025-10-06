using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemFulfillment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderItemFulfillments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickingListItemId = table.Column<int>(type: "int", nullable: false),
                    FulfillmentType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FulfilledQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FulfillmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoadId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemFulfillments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemFulfillments_AspNetUsers_RecordedById",
                        column: x => x.RecordedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemFulfillments_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_OrderItemFulfillments_PickingListItems_PickingListItemId",
                        column: x => x.PickingListItemId,
                        principalTable: "PickingListItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemFulfillments_LoadId",
                table: "OrderItemFulfillments",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemFulfillments_PickingListItemId",
                table: "OrderItemFulfillments",
                column: "PickingListItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemFulfillments_RecordedById",
                table: "OrderItemFulfillments",
                column: "RecordedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemFulfillments");
        }
    }
}
