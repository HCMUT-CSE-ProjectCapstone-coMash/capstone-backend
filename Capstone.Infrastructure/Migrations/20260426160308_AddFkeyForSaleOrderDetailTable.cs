using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFkeyForSaleOrderDetailTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ComboPromotionId",
                table: "sale_order_details",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProductPromotionId",
                table: "sale_order_details",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_sale_order_details_ComboPromotionId",
                table: "sale_order_details",
                column: "ComboPromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_sale_order_details_ProductPromotionId",
                table: "sale_order_details",
                column: "ProductPromotionId");

            migrationBuilder.AddForeignKey(
                name: "FK_sale_order_details_combo_promotions_ComboPromotionId",
                table: "sale_order_details",
                column: "ComboPromotionId",
                principalTable: "combo_promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sale_order_details_product_promotions_ProductPromotionId",
                table: "sale_order_details",
                column: "ProductPromotionId",
                principalTable: "product_promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sale_order_details_combo_promotions_ComboPromotionId",
                table: "sale_order_details");

            migrationBuilder.DropForeignKey(
                name: "FK_sale_order_details_product_promotions_ProductPromotionId",
                table: "sale_order_details");

            migrationBuilder.DropIndex(
                name: "IX_sale_order_details_ComboPromotionId",
                table: "sale_order_details");

            migrationBuilder.DropIndex(
                name: "IX_sale_order_details_ProductPromotionId",
                table: "sale_order_details");

            migrationBuilder.DropColumn(
                name: "ComboPromotionId",
                table: "sale_order_details");

            migrationBuilder.DropColumn(
                name: "ProductPromotionId",
                table: "sale_order_details");
        }
    }
}
