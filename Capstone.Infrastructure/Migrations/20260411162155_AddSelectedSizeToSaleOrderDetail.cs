using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectedSizeToSaleOrderDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedSize",
                table: "sale_order_details",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedSize",
                table: "sale_order_details");
        }
    }
}
