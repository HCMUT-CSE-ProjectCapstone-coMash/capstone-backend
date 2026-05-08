using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToTemporaryProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "temporary_products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_temporary_products_CreatedBy",
                table: "temporary_products",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_temporary_products_users_CreatedBy",
                table: "temporary_products",
                column: "CreatedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_temporary_products_users_CreatedBy",
                table: "temporary_products");

            migrationBuilder.DropIndex(
                name: "IX_temporary_products_CreatedBy",
                table: "temporary_products");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "temporary_products");
        }
    }
}
