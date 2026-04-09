using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreFieldForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DateOfBirth",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "users");
        }
    }
}
