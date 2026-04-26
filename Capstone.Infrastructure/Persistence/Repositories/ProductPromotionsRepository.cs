using Capstone.Application.Common.Interfaces.Persistence;
using Capstone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Infrastructure.Persistence.Repositories;

public class ProductPromotionsRepository : IProductPromotionsRepository
{
    private readonly AppDbContext _context;

    public ProductPromotionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateProductPromotion(ProductPromotion productPromotion)
    {
        _context.ProductPromotions.Add(productPromotion);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ProductPromotion>> GetProductPromotionsByPromotionId(Guid promotionId)
    {
        return await _context.ProductPromotions
            .AsNoTracking()
            .Include(pp => pp.Product)
                .ThenInclude(p => p.ProductQuantities)
            .Where(pp => pp.PromotionId == promotionId)
            .ToListAsync();
    }

    public async Task DeleteProductPromotionsByPromotionId(Guid promotionId)
    {
        var productPromotions = await _context.ProductPromotions
            .Where(pp => pp.PromotionId == promotionId)
            .ToListAsync();

        if (productPromotions.Any())
        {
            _context.ProductPromotions.RemoveRange(productPromotions);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ProductPromotion?> GetProductPromotionById(Guid productPromotionId)
    {
        return await _context.ProductPromotions
            .AsNoTracking()
            .Include(pp => pp.Product)
                .ThenInclude(p => p.ProductQuantities)
            .FirstOrDefaultAsync(pp => pp.Id == productPromotionId);
    }
}