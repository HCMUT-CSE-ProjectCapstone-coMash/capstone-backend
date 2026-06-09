using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTemporaryProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "temporary_products");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "temporary_products");

            migrationBuilder.DropColumn(
                name: "Pattern",
                table: "temporary_products");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "temporary_products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ColorId",
                table: "temporary_products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PatternId",
                table: "temporary_products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_temporary_products_CategoryId",
                table: "temporary_products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_temporary_products_ColorId",
                table: "temporary_products",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_temporary_products_PatternId",
                table: "temporary_products",
                column: "PatternId");

            migrationBuilder.AddForeignKey(
                name: "FK_temporary_products_categories_CategoryId",
                table: "temporary_products",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_temporary_products_colors_ColorId",
                table: "temporary_products",
                column: "ColorId",
                principalTable: "colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_temporary_products_patterns_PatternId",
                table: "temporary_products",
                column: "PatternId",
                principalTable: "patterns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_temporary_products_categories_CategoryId",
                table: "temporary_products");

            migrationBuilder.DropForeignKey(
                name: "FK_temporary_products_colors_ColorId",
                table: "temporary_products");

            migrationBuilder.DropForeignKey(
                name: "FK_temporary_products_patterns_PatternId",
                table: "temporary_products");

            migrationBuilder.DropIndex(
                name: "IX_temporary_products_CategoryId",
                table: "temporary_products");

            migrationBuilder.DropIndex(
                name: "IX_temporary_products_ColorId",
                table: "temporary_products");

            migrationBuilder.DropIndex(
                name: "IX_temporary_products_PatternId",
                table: "temporary_products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "temporary_products");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "temporary_products");

            migrationBuilder.DropColumn(
                name: "PatternId",
                table: "temporary_products");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "temporary_products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "temporary_products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Pattern",
                table: "temporary_products",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
