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
    public DbSet<SaleOrder> SaleOrders => Set<SaleOrder>();
    public DbSet<SaleOrderDetail> SaleOrderDetails => Set<SaleOrderDetail>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<ProductPromotion> ProductPromotions => Set<ProductPromotion>();
    public DbSet<OrderPromotion> OrderPromotions => Set<OrderPromotion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User Table
        modelBuilder.Entity<User>().ToTable("users");

        // Product Table
        modelBuilder.Entity<Product>(enity =>
        {
            enity.ToTable("products");

            // Product has one User (CreatedBy) and User has many Products
            enity.HasOne<User>().WithMany().HasForeignKey(p => p.CreatedBy).OnDelete(DeleteBehavior.Restrict); ;
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
            entity.HasOne<User>().WithMany().HasForeignKey(po => po.CreatedBy).OnDelete(DeleteBehavior.Restrict); ;
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

        // SaleOrder Table
        modelBuilder.Entity<SaleOrder>(entity =>
        {
            entity.ToTable("sale_orders");

            // SaleOrder has one User (CreatedBy) and User has many SaleOrders
            entity.HasOne(so => so.User).WithMany().HasForeignKey(so => so.CreatedBy).OnDelete(DeleteBehavior.Restrict); ;

            // SaleOrder has one Customer and Customer has many SaleOrders
            entity.HasOne(so => so.Customer).WithMany(c => c.SaleOrders).HasForeignKey(so => so.CustomerId).IsRequired(false);
        });

        // SaleOrderDetail Table
        modelBuilder.Entity<SaleOrderDetail>(entity =>
        {
            entity.ToTable("sale_order_details");

            // SaleOrderDetail has one SaleOrder and SaleOrder has many SaleOrderDetails
            entity.HasOne(sod => sod.SaleOrder).WithMany(so => so.SaleOrderDetails).HasForeignKey(sod => sod.SaleOrderId).OnDelete(DeleteBehavior.Cascade);

            // SaleOrderDetail has one Product and Product has many SaleOrderDetails
            entity.HasOne(sod => sod.Product).WithMany().HasForeignKey(sod => sod.ProductId);
        });

        // Promotion Table
        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.ToTable("promotions");

            // Promotion has one User (CreatedBy) and User has many Promotions
            entity.HasOne(p => p.User).WithMany().HasForeignKey(p => p.CreatedBy).OnDelete(DeleteBehavior.Restrict);

            // Promotion has many ProductPromotions and ProductPromotion has one Promotion
            entity.HasMany(p => p.ProductPromotions).WithOne(pp => pp.Promotion).HasForeignKey(pp => pp.PromotionId).OnDelete(DeleteBehavior.Cascade);

            // Promotion has many OrderPromotions and OrderPromotion has one Promotion
            entity.HasMany(p => p.OrderPromotions).WithOne(op => op.Promotion).HasForeignKey(op => op.PromotionId).OnDelete(DeleteBehavior.Cascade);
        });

        // ProductPromotion Table
        modelBuilder.Entity<ProductPromotion>(entity =>
        {
            entity.ToTable("product_promotions");

            // ProductPromotion has one Product and Product has many ProductPromotions
            entity.HasOne(pp => pp.Product).WithMany().HasForeignKey(pp => pp.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        // OrderPromotion Table
        modelBuilder.Entity<OrderPromotion>(entity =>
        {
            entity.ToTable("order_promotions");
        });
    }
}