using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class ProductQuantitiesRepository : IProductQuantitiesRepository
{
    private readonly AppDbContext _context;

    public ProductQuantitiesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddProductQuantities(ProductQuantity productQuantities)
    {
        _context.ProductQuantities.Add(productQuantities);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProductQuantity(ProductQuantity productQuantity)
    {
        _context.ProductQuantities.Update(productQuantity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProductQuantitiesByProductId(Guid productId)
    {
        var existing = await _context.ProductQuantities.Where(pq => pq.ProductId == productId).ToListAsync();

        if (existing.Count == 0)
        {
            return;
        }

        _context.ProductQuantities.RemoveRange(existing);
        await _context.SaveChangesAsync();
    }

    public async Task<ProductQuantity?> GetProductQuantitiesByProductId(Guid productId, string size)
    {
        return await _context.ProductQuantities.FirstOrDefaultAsync(pq => pq.ProductId == productId && pq.Size == size);
    }
}