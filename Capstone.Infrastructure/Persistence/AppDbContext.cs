using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Capstone.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductQuantity> ProductQuantities => Set<ProductQuantity>();
    public DbSet<ProductsOrder> ProductsOrders => Set<ProductsOrder>();
    public DbSet<ProductsOrdersDetail> ProductsOrdersDetails => Set<ProductsOrdersDetail>();
    public DbSet<ProductsOrdersDetailQuantityChange> ProductsOrdersDetailQuantityChanges => Set<ProductsOrdersDetailQuantityChange>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User Table
        modelBuilder.Entity<User>().ToTable("users");

        // Product Table
        modelBuilder.Entity<Product>(enity =>
        {
            enity.ToTable("products");

            // Product has one User (CreatedBy) and User has many Products
            enity.HasOne<User>().WithMany().HasForeignKey(p => p.CreatedBy);
        });

        // ProductQuantities Table
        modelBuilder.Entity<ProductQuantity>(entity =>
        {
            entity.ToTable("product_quantities");

            // ProductQuantities has one Product and Product has many ProductQuantities
            entity.HasOne(pq => pq.Product).WithMany(p => p.ProductQuantities).HasForeignKey(pq => pq.ProductId).OnDelete(DeleteBehavior.Cascade); ;
        });

        // ProductsOrder Table
        modelBuilder.Entity<ProductsOrder>(entity =>
        {
            entity.ToTable("products_orders");

            // ProductsOrder has one User (CreatedBy) and User has many ProductsOrders
            entity.HasOne<User>().WithMany().HasForeignKey(po => po.CreatedBy);
        });

        // ProductsOrdersDetail Table
        modelBuilder.Entity<ProductsOrdersDetail>(entity =>
        {
            entity.ToTable("products_orders_details");

            // ProductsOrdersDetail has one ProductsOrder and ProductsOrder has many ProductsOrdersDetails
            entity.HasOne<ProductsOrder>().WithMany(po => po.ProductsOrdersDetails).HasForeignKey(pod => pod.ProductsOrderId).OnDelete(DeleteBehavior.Cascade);

            // ProductsOrdersDetail has one Product and Product has many ProductsOrdersDetails
            entity.HasOne(d => d.Product).WithMany().HasForeignKey(pod => pod.ProductId);
        });

        // ProductsOrdersDetailQuantityChange Table
        modelBuilder.Entity<ProductsOrdersDetailQuantityChange>(entity =>
        {
            entity.ToTable("products_orders_detail_quantity_changes");

            // ProductsOrdersDetailQuantityChange has one ProductsOrdersDetail and ProductsOrdersDetail has many ProductsOrdersDetailQuantityChanges
            entity.HasOne(podqc => podqc.ProductsOrdersDetail).WithMany(pod => pod.QuantityChanges).HasForeignKey(podqc => podqc.ProductsOrdersDetailId).OnDelete(DeleteBehavior.Cascade);
        });

        // Customer Table
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
        });
    }
}