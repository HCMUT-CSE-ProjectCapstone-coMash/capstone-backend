using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Common;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class ProductsRepository : IProductsRepository
{
    private readonly AppDbContext _context;

    public ProductsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddProduct(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
    }

    public async Task<Product?> GetProductById(Guid id)
    {
        return await _context.Products
            .Include(p => p.ProductQuantities)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task UpdateProduct(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Product>> GetPendingProductsByUserId(Guid userId)
    {
        return await _context.Products
            .Include(p => p.ProductQuantities) 
            .Where(p => p.CreatedBy == userId && p.Status == ProductStatus.Pending)
            .ToListAsync();
    }
}