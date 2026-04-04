using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductsOrdersDetailQuantityChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "products_orders_detail_quantity_changes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductsOrdersDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldQuantity = table.Column<int>(type: "integer", nullable: false),
                    NewQuantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products_orders_detail_quantity_changes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_orders_detail_quantity_changes_products_orders_det~",
                        column: x => x.ProductsOrdersDetailId,
                        principalTable: "products_orders_details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_orders_detail_quantity_changes_ProductsOrdersDetai~",
                table: "products_orders_detail_quantity_changes",
                column: "ProductsOrdersDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "products_orders_detail_quantity_changes");
        }
    }
}
