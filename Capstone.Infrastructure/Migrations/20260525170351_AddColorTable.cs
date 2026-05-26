using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Pattern",
                table: "products");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ColorId",
                table: "products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PatternId",
                table: "products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "colors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ColorName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_colors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_CategoryId",
                table: "products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_products_ColorId",
                table: "products",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_products_PatternId",
                table: "products",
                column: "PatternId");

            migrationBuilder.AddForeignKey(
                name: "FK_products_categories_CategoryId",
                table: "products",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_products_colors_ColorId",
                table: "products",
                column: "ColorId",
                principalTable: "colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_products_patterns_PatternId",
                table: "products",
                column: "PatternId",
                principalTable: "patterns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_categories_CategoryId",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_products_colors_ColorId",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_products_patterns_PatternId",
                table: "products");

            migrationBuilder.DropTable(
                name: "colors");

            migrationBuilder.DropIndex(
                name: "IX_products_CategoryId",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_ColorId",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_PatternId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "PatternId",
                table: "products");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Pattern",
                table: "products",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
