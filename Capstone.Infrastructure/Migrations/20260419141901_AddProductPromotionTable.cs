using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductPromotionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "promotions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "promotions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "product_promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscountType = table.Column<string>(type: "text", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_promotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_promotions_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_promotions_promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_promotions_CreatedBy",
                table: "promotions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_product_promotions_ProductId",
                table: "product_promotions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_promotions_PromotionId",
                table: "product_promotions",
                column: "PromotionId");

            migrationBuilder.AddForeignKey(
                name: "FK_promotions_users_CreatedBy",
                table: "promotions",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_promotions_users_CreatedBy",
                table: "promotions");

            migrationBuilder.DropTable(
                name: "product_promotions");

            migrationBuilder.DropIndex(
                name: "IX_promotions_CreatedBy",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "promotions");
        }
    }
}
