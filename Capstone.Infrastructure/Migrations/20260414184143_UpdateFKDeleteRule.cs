using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFKDeleteRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_users_CreatedBy",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_products_orders_users_CreatedBy",
                table: "products_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_sale_orders_users_CreatedBy",
                table: "sale_orders");

            migrationBuilder.AddForeignKey(
                name: "FK_products_users_CreatedBy",
                table: "products",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_products_orders_users_CreatedBy",
                table: "products_orders",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sale_orders_users_CreatedBy",
                table: "sale_orders",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_users_CreatedBy",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_products_orders_users_CreatedBy",
                table: "products_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_sale_orders_users_CreatedBy",
                table: "sale_orders");

            migrationBuilder.AddForeignKey(
                name: "FK_products_users_CreatedBy",
                table: "products",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_products_orders_users_CreatedBy",
                table: "products_orders",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sale_orders_users_CreatedBy",
                table: "sale_orders",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
