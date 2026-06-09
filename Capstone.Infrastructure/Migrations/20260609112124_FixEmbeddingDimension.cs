using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEmbeddingDimension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VectorId",
                table: "products");

            migrationBuilder.AlterColumn<Vector>(
                name: "Embedding",
                table: "products",
                type: "vector(512)",
                nullable: false,
                oldClrType: typeof(Vector),
                oldType: "vector",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Vector>(
                name: "Embedding",
                table: "products",
                type: "vector",
                nullable: true,
                oldClrType: typeof(Vector),
                oldType: "vector(512)");

            migrationBuilder.AddColumn<string>(
                name: "VectorId",
                table: "products",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
