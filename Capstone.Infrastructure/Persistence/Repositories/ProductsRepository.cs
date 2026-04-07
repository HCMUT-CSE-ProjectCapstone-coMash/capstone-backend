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

    public async Task UpdateProduct(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task<Product?> GetProductById(Guid productId)
    {
        return await _context.Products.Include(p => p.ProductQuantities).FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task DeleteProductAsync(Guid productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetMaxIdNumberByCategoryAsync(string prefix)
    {
        var numberStrings = await _context.Products
            .Where(p => p.ProductId.StartsWith(prefix + "-"))
            .Select(p => p.ProductId.Substring(prefix.Length + 1))
            .ToListAsync();

        if (!numberStrings.Any()) return 0;

        return numberStrings.Select(n => int.TryParse(n, out var num) ? num : 0).Max();
    }

    public Task<List<Product>> FetchApprovedProductByName(string productName)
    {
        var searchPattern = $"%{productName}%";

        return _context.Products
            .Include(p => p.ProductQuantities)
            .Where(p => p.Status == ProductStatus.Approved &&
                EF.Functions.ILike(
                    EF.Functions.Unaccent(p.ProductName),
                    EF.Functions.Unaccent(searchPattern)
                ))
            .Take(8)
            .ToListAsync();
    }

    public async Task<(List<Product> Items, int Total)> FetchAllProducts(int page, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.ProductQuantities)
            .Where(p => p.Status == ProductStatus.Approved)
            .AsQueryable();

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}