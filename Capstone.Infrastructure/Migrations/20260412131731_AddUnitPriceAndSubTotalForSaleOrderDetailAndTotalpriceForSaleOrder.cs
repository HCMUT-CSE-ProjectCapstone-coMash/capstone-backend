using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitPriceAndSubTotalForSaleOrderDetailAndTotalpriceForSaleOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TotalPrice",
                table: "sale_orders",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SubTotal",
                table: "sale_order_details",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "UnitPrice",
                table: "sale_order_details",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "sale_orders");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "sale_order_details");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "sale_order_details");
        }
    }
}
