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
        string? parsedProductId = null;
        string? parsedSize = null;

        var lastDash = productName.LastIndexOf('-');
        if (lastDash > 0)
        {
            parsedProductId = productName[..lastDash];
            parsedSize      = productName[(lastDash + 1)..];
        }

        var searchPattern = $"%{productName}%";

        return _context.Products
            .Include(p => p.ProductQuantities)
            .Where(p => p.Status == ProductStatus.Approved &&
                (
                    EF.Functions.ILike(
                        EF.Functions.Unaccent(p.ProductName),
                        EF.Functions.Unaccent(searchPattern))
                    ||
                    EF.Functions.ILike(p.ProductId, searchPattern)
                    ||
                    (parsedProductId != null && parsedSize != null &&
                    EF.Functions.ILike(p.ProductId, $"%{parsedProductId}%") &&
                    p.ProductQuantities.Any(q => EF.Functions.ILike(q.Size, parsedSize)))
                ))
            .Take(8)
            .ToListAsync();
    }

    public async Task<(List<Product> Items, int Total)> FetchAllProducts(int page, int pageSize, string? category = null, string? search = null)
    {
        string? parsedProductId = null;
        string? parsedSize = null;

        if (!string.IsNullOrEmpty(search))
        {
            var lastDash = search.LastIndexOf("-");
            if (lastDash > 0)
            {
                parsedProductId = search[..lastDash];
                parsedSize      = search[(lastDash + 1)..];
            }
        }

        var query = _context.Products
            .Include(p => p.ProductQuantities)
            .Where(p => p.Status == ProductStatus.Approved)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchPattern = $"%{search}%";
            query = query.Where(p =>
                // match full search term against name
                EF.Functions.ILike(
                    EF.Functions.Unaccent(p.ProductName),
                    EF.Functions.Unaccent(searchPattern))
                ||
                // match "VAY-6" part against productId
                EF.Functions.ILike(p.ProductId, searchPattern)
                ||
                // match "L" part against a quantity size
                (parsedProductId != null && parsedSize != null &&
                EF.Functions.ILike(p.ProductId, $"%{parsedProductId}%") &&
                p.ProductQuantities.Any(q => EF.Functions.ILike(q.Size, parsedSize)))
            );
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}