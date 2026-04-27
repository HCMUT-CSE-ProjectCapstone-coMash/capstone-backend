using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFKeyFromSaleOrderToOrderPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AppliedOrderPromotionId",
                table: "sale_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "MinValue",
                table: "order_promotions",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "MaxDiscount",
                table: "order_promotions",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<double>(
                name: "DiscountValue",
                table: "order_promotions",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.CreateIndex(
                name: "IX_sale_orders_AppliedOrderPromotionId",
                table: "sale_orders",
                column: "AppliedOrderPromotionId");

            migrationBuilder.AddForeignKey(
                name: "FK_sale_orders_order_promotions_AppliedOrderPromotionId",
                table: "sale_orders",
                column: "AppliedOrderPromotionId",
                principalTable: "order_promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sale_orders_order_promotions_AppliedOrderPromotionId",
                table: "sale_orders");

            migrationBuilder.DropIndex(
                name: "IX_sale_orders_AppliedOrderPromotionId",
                table: "sale_orders");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "HasChangedPassword",
                table: "users");

            migrationBuilder.DropColumn(
                name: "AppliedOrderPromotionId",
                table: "sale_orders");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinValue",
                table: "order_promotions",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxDiscount",
                table: "order_promotions",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountValue",
                table: "order_promotions",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
