using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProductEmbeddingType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float[]>(
                name: "Embedding",
                table: "products",
                type: "vector(512)",
                nullable: true,
                oldClrType: typeof(float[]),
                oldType: "vector(512)",
                oldNullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float[]>(
                name: "Embedding",
                table: "products",
                type: "vector(512)",
                nullable: false,
                oldClrType: typeof(float[]),
                oldType: "vector(512)",
                oldNullable: true);
        }
    }
}
