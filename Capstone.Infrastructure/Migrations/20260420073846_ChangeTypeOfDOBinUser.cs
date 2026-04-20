using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTypeOfDOBinUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "users");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "users",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1900, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "users");

            migrationBuilder.AddColumn<string>(
                name: "DateOfBirth",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
