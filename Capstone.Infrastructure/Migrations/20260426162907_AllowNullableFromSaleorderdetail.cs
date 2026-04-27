using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullableFromSaleorderdetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ProductPromotionId",
                table: "sale_order_details",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ComboPromotionId",
                table: "sale_order_details",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FK constraints first so we can fill NULLs freely
            migrationBuilder.DropForeignKey(
                name: "FK_sale_order_details_product_promotions_ProductPromotionId",
                table: "sale_order_details");

            migrationBuilder.DropForeignKey(
                name: "FK_sale_order_details_combo_promotions_ComboPromotionId",
                table: "sale_order_details");

            // Delete rows with NULL since there's no valid FK value to fill with
            migrationBuilder.Sql(@"
                DELETE FROM sale_order_details 
                WHERE ""ProductPromotionId"" IS NULL OR ""ComboPromotionId"" IS NULL;
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductPromotionId",
                table: "sale_order_details",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ComboPromotionId",
                table: "sale_order_details",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // Re-add FK constraints
            migrationBuilder.AddForeignKey(
                name: "FK_sale_order_details_product_promotions_ProductPromotionId",
                table: "sale_order_details",
                column: "ProductPromotionId",
                principalTable: "product_promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sale_order_details_combo_promotions_ComboPromotionId",
                table: "sale_order_details",
                column: "ComboPromotionId",
                principalTable: "combo_promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
