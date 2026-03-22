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

    public async Task AddProductQuantities(ProductQuantities productQuantities)
    {
        _context.ProductQuantities.Add(productQuantities);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByProductId(Guid productId)
    {
        var existing = await _context.ProductQuantities
            .Where(pq => pq.ProductID == productId)
            .ToListAsync();

        if (existing.Count == 0)
        {
            return;
        }

        _context.ProductQuantities.RemoveRange(existing);
        await _context.SaveChangesAsync();
    }
}