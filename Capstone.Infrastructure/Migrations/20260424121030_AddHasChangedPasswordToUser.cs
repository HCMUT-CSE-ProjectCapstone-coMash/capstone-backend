using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHasChangedPasswordToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE users DROP COLUMN IF EXISTS \"HasChangedPassword\";");

            migrationBuilder.AddColumn<bool>(
                name: "HasChangedPassword",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasChangedPassword",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "HasChangedPassword",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "False");
        }
            }
    
}
