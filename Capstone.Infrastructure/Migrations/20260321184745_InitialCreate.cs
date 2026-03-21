using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Capstone.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<string>(type: "text", nullable: false),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    Pattern = table.Column<string>(type: "text", nullable: false),
                    SizeType = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ImageKey = table.Column<string>(type: "text", nullable: false),
                    VectorId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "products_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderName = table.Column<string>(type: "text", nullable: false),
                    OrderDescription = table.Column<string>(type: "text", nullable: false),
                    OrderStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_orders_users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_quantities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Size = table.Column<string>(type: "text", nullable: false),
                    Quantities = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_quantities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_quantities_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "products_orders_details",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductsOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products_orders_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_orders_details_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_products_orders_details_products_orders_ProductsOrderId",
                        column: x => x.ProductsOrderId,
                        principalTable: "products_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_quantities_ProductId",
                table: "product_quantities",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_products_CreatedBy",
                table: "products",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_products_orders_CreatedBy",
                table: "products_orders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_products_orders_details_ProductId",
                table: "products_orders_details",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_products_orders_details_ProductsOrderId",
                table: "products_orders_details",
                column: "ProductsOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_quantities");

            migrationBuilder.DropTable(
                name: "products_orders_details");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "products_orders");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
