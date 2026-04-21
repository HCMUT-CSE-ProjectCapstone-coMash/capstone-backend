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
}